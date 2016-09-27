#region Usings

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Apache.NMS;
using Apache.NMS.Stomp.Protocol;
using Apache.NMS.Stomp.Transport;
using Extend;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Factory for TCP/IP transport.
    /// </summary>
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

        #region Protected Members

        /// <summary>
        ///     Override in a subclass to create the specific type of transport that is
        ///     being implemented.
        /// </summary>
        protected virtual ITransport CreateTransport( Uri location, Socket socket, IWireFormat wireFormat )
            => new TcpTransport( location, socket, wireFormat );

        #endregion

        #region Implementation of ITransportFactory

        public ITransport CompositeConnect( Uri location )
        {
            var socket = Connect( location.Host, location.Port );
            socket.ReceiveBufferSize = _stompConnectionSettings.TransportSettings.ReceiveBufferSize;
            socket.SendBufferSize = _stompConnectionSettings.TransportSettings.SendBufferSize;
            socket.ReceiveTimeout = _stompConnectionSettings.TransportSettings.ReceiveTimeout;
            socket.SendTimeout = _stompConnectionSettings.TransportSettings.SendTimeout;

            var wireformat = new StompWireFormat();
            var transport = CreateTransport( location, socket, wireformat );
            wireformat.Transport = transport;

            if ( _stompConnectionSettings.TransportSettings.UseLogging )
                transport = new LoggingTransport( transport );

            if ( _stompConnectionSettings.TransportSettings.UseInactivityMonitor )
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

        #region Private Members

        /// <summary>
        ///     Tries to create an open socket to the host with the given name/IP and port.
        /// </summary>
        /// <exception cref="NmsConnectionException">Connection failed.</exception>
        /// <exception cref="ArgumentNullException">host can not be null.</exception>
        /// <param name="host">The host name.</param>
        /// <param name="port">The port.</param>
        /// <returns>Returns an open socket, or null if the communication has failed.</returns>
        [NotNull]
        private static Socket Connect( [NotNull] String host, Int32 port )
        {
            host.ThrowIfNull( nameof( host ) );

            try
            {
                Socket socket = null;

                // Check if is IP address
                IPAddress ipAddress;
                if ( IPAddress.TryParse( host, out ipAddress ) )
                {
                    socket = ConnectSocket( ipAddress, port );
                    if ( socket != null )
                        return socket;
                }

                // host must be a host name
                // Try to get the DNS entry of the host
                var hostEntry = GetHostEntry( host );
                if ( null == hostEntry )
                    throw new SocketException();

                // Looping through the AddressList allows different type of connections to be tried (IPv6, IPv4 and whatever else may be available).
                // Prefer IPv6 first.
                ipAddress = GetIpAddress( hostEntry, AddressFamily.InterNetworkV6 );
                if ( ipAddress != null )
                    socket = ConnectSocket( ipAddress, port );
                if ( socket != null )
                    return socket;

                // Try IPv4 next.
                ipAddress = GetIpAddress( hostEntry, AddressFamily.InterNetwork );
                if ( ipAddress != null )
                    socket = ConnectSocket( ipAddress, port );
                if ( socket != null )
                    return socket;

                foreach ( var address in hostEntry.AddressList.Where( x => AddressFamily.InterNetworkV6 != x.AddressFamily && AddressFamily.InterNetwork != x.AddressFamily ) )
                {
                    socket = ConnectSocket( address, port );
                    if ( null != socket )
                        return socket;
                }

                throw new SocketException();
            }
            catch ( Exception ex )
            {
                throw new NmsConnectionException( $"Error connecting to {host}:{port}.", ex );
            }
        }

        /// <summary>
        ///     Gets the first address in the address list of the given host entry of the specified address family.
        /// </summary>
        /// <param name="hostEntry">The host entry.</param>
        /// <param name="addressFamily">The address family.</param>
        /// <returns>Returns the first matching address, or null if none was found.</returns>
        [CanBeNull]
        private static IPAddress GetIpAddress( [CanBeNull] IPHostEntry hostEntry, AddressFamily addressFamily )
            => hostEntry?.AddressList.FirstOrDefault( address => address.AddressFamily == addressFamily );

        /// <summary>
        ///     Creates and connects a new socket to the given endpoint.
        /// </summary>
        /// <param name="address">The endpoint address.</param>
        /// <param name="port">The endpoint port.</param>
        /// <returns>Returns the connected socket, or null if the connection failed.</returns>
        [CanBeNull]
        private static Socket ConnectSocket( [NotNull] IPAddress address, Int32 port )
        {
            try
            {
                var socket = new Socket( address.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
                socket.Connect( new IPEndPoint( address, port ) );

                if ( socket.Connected )
                    return socket;
            }
            catch
            {
                // ignored
            }
            return null;
        }

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
    }
}