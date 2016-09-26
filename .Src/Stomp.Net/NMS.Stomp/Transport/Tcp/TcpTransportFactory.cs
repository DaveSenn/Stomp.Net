#region Usings

using System;
using System.Net;
using System.Net.Sockets;
using Apache.NMS.Stomp.Protocol;
using Apache.NMS.Util;
using Extend;
using JetBrains.Annotations;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    public class TcpTransportFactory : ITransportFactory
    {
        #region Fields

        /// <summary>
        ///     The STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TcpTransportFactory" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">stompConnectionSettings can not be null.</exception>
        /// <param name="stompConnectionSettings">Some STOMP settings.</param>
        public TcpTransportFactory( [NotNull] StompConnectionSettings stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        public static IPAddress GetIPAddress( String hostname, AddressFamily addressFamily )
        {
            IPAddress ipaddress = null;
            var hostEntry = GetHostEntry( hostname );

            if ( null != hostEntry )
                ipaddress = GetIPAddress( hostEntry, addressFamily );

            return ipaddress;
        }

        public static IPAddress GetIPAddress( IPHostEntry hostEntry, AddressFamily addressFamily )
        {
            if ( null != hostEntry )
                foreach ( var address in hostEntry.AddressList )
                    if ( address.AddressFamily == addressFamily )
                        return address;

            return null;
        }

        public static Boolean TryParseIPAddress( String host, out IPAddress ipaddress )
        {
#if !NETCF
            return IPAddress.TryParse( host, out ipaddress );
#else
            try
            {
                ipaddress = IPAddress.Parse(host);
            }
            catch
            {
                ipaddress = null;
            }

            return (null != ipaddress);
#endif
        }

        protected Socket Connect( String host, Int32 port )
        {
            Socket socket = null;
            IPAddress ipaddress;

            try
            {
                if ( TryParseIPAddress( host, out ipaddress ) )
                {
                    socket = ConnectSocket( ipaddress, port );
                }
                else
                {
                    // Looping through the AddressList allows different type of connections to be tried
                    // (IPv6, IPv4 and whatever else may be available).
                    var hostEntry = GetHostEntry( host );

                    if ( null != hostEntry )
                    {
                        // Prefer IPv6 first.
                        ipaddress = GetIPAddress( hostEntry, AddressFamily.InterNetworkV6 );
                        socket = ConnectSocket( ipaddress, port );
                        if ( null == socket )
                        {
                            // Try IPv4 next.
                            ipaddress = GetIPAddress( hostEntry, AddressFamily.InterNetwork );
                            socket = ConnectSocket( ipaddress, port );
                            if ( null == socket )
                                foreach ( var address in hostEntry.AddressList )
                                {
                                    if ( AddressFamily.InterNetworkV6 == address.AddressFamily
                                         || AddressFamily.InterNetwork == address.AddressFamily )
                                        continue;

                                    socket = ConnectSocket( address, port );
                                    if ( null != socket )
                                    {
                                        ipaddress = address;
                                        break;
                                    }
                                }
                        }
                    }
                }

                if ( null == socket )
                    throw new SocketException();
            }
            catch ( Exception ex )
            {
                throw new NMSConnectionException( String.Format( "Error connecting to {0}:{1}.", host, port ), ex );
            }

            Tracer.DebugFormat( "Connected to {0}:{1} using {2} protocol.", host, port, ipaddress.AddressFamily.ToString() );
            return socket;
        }

        /// <summary>
        ///     Override in a subclass to create the specific type of transport that is
        ///     being implemented.
        /// </summary>
        protected virtual ITransport DoCreateTransport( Uri location, Socket socket, IWireFormat wireFormat ) => new TcpTransport( location, socket, wireFormat );

        private Socket ConnectSocket( IPAddress address, Int32 port )
        {
            if ( null != address )
                try
                {
                    var socket = new Socket( address.AddressFamily, SocketType.Stream, ProtocolType.Tcp );

                    if ( null != socket )
                    {
                        socket.Connect( new IPEndPoint( address, port ) );
                        if ( socket.Connected )
                            return socket;
                    }
                }
                catch
                {
                }

            return null;
        }

        #region Private Members

        /// <summary>
        ///     Gets the DNS entry of the host with the given name.
        /// </summary>
        /// <param name="host">The name of the host.</param>
        /// <returns>returns the DNS entry, or null if not found.</returns>
        [CanBeNull]
        private static IPHostEntry GetHostEntry( [NotNull] String host )
        {
            try
            {
                return Dns.GetHostEntry( host );
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region ITransportFactory Members

        /*
        public ITransport CompositeConnect( Uri location )
            => CompositeConnect( location, null );
        */

        public ITransport CompositeConnect( Uri location )
        {
            // Extract query parameters from broker Uri
            var map = URISupport.ParseQuery( location.Query );
            
            var socket = Connect( location.Host, location.Port );
            socket.ReceiveBufferSize = _stompConnectionSettings.TransportSettings.ReceiveBufferSize;
            socket.SendBufferSize = _stompConnectionSettings.TransportSettings.SendBufferSize;
            socket.ReceiveTimeout = _stompConnectionSettings.TransportSettings.ReceiveTimeout;
            socket.SendTimeout = _stompConnectionSettings.TransportSettings.SendTimeout;

            var wireformat = new StompWireFormat();
            // Set wireformat. properties on the wireformat owned by the tcpTransport
            URISupport.SetProperties( wireformat, map, "wireFormat." );
            var transport = DoCreateTransport( location, socket, wireformat );

            wireformat.Transport = transport;

            if (_stompConnectionSettings.TransportSettings.UseLogging )
                transport = new LoggingTransport( transport );

            if (_stompConnectionSettings.TransportSettings.UseInactivityMonitor )
                transport = new InactivityMonitor( transport, wireformat );
            
            return transport;
        }

        public ITransport CreateTransport( Uri location )
        {
            var transport = CompositeConnect( location );

            transport = new MutexTransport( transport );
            transport = new ResponseCorrelator( transport );

            return transport;
        }

        #endregion
    }
}