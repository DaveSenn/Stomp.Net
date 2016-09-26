

#region Usings

using System;
using System.Net;
using System.Net.Sockets;
using Apache.NMS.Stomp.Protocol;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    public class TcpTransportFactory : ITransportFactory
    {
        #region Properties

        public Boolean UseLogging { get; set; }

        /// <summary>
        ///     Should the Inactivity Monitor be enabled on this Transport.
        /// </summary>
        private Boolean useInactivityMonitor = true;

        public Boolean UseInactivityMonitor
        {
            get { return useInactivityMonitor; }
            set { useInactivityMonitor = value; }
        }

        /// <summary>
        ///     Size in bytes of the receive buffer.
        /// </summary>
        private Int32 receiveBufferSize = 8192;

        public Int32 ReceiveBufferSize
        {
            get { return receiveBufferSize; }
            set { receiveBufferSize = value; }
        }

        /// <summary>
        ///     Size in bytes of send buffer.
        /// </summary>
        private Int32 sendBufferSize = 8192;

        public Int32 SendBufferSize
        {
            get { return sendBufferSize; }
            set { sendBufferSize = value; }
        }

        /// <summary>
        ///     The time-out value, in milliseconds. The default value is 0, which indicates
        ///     an infinite time-out period. Specifying -1 also indicates an infinite time-out period.
        /// </summary>
        private Int32 receiveTimeout;

        public Int32 ReceiveTimeout
        {
            get { return receiveTimeout; }
            set { receiveTimeout = value; }
        }

        /// <summary>
        ///     The time-out value, in milliseconds. If you set the property with a value between 1 and 499,
        ///     the value will be changed to 500. The default value is 0, which indicates an infinite
        ///     time-out period. Specifying -1 also indicates an infinite time-out period.
        /// </summary>
        private Int32 sendTimeout;

        public Int32 SendTimeout
        {
            get { return sendTimeout; }
            set { sendTimeout = value; }
        }

        #endregion

        #region ITransportFactory Members

        public ITransport CompositeConnect( Uri location ) => CompositeConnect( location, null );

        public ITransport CompositeConnect( Uri location, SetTransport setTransport )
        {
            // Extract query parameters from broker Uri
            var map = URISupport.ParseQuery( location.Query );

            // Set transport. properties on this (the factory)
            URISupport.SetProperties( this, map, "transport." );

            Tracer.Debug( "Opening socket to: " + location.Host + " on port: " + location.Port );
            var socket = Connect( location.Host, location.Port );

#if !NETCF
            socket.ReceiveBufferSize = ReceiveBufferSize;
            socket.SendBufferSize = SendBufferSize;
            socket.ReceiveTimeout = ReceiveTimeout;
            socket.SendTimeout = SendTimeout;
#endif

            var wireformat = new StompWireFormat();
            // Set wireformat. properties on the wireformat owned by the tcpTransport
            URISupport.SetProperties( wireformat, map, "wireFormat." );
            var transport = DoCreateTransport( location, socket, wireformat );

            wireformat.Transport = transport;

            if ( UseLogging )
                transport = new LoggingTransport( transport );

            if ( UseInactivityMonitor )
                transport = new InactivityMonitor( transport, wireformat );

            if ( setTransport != null )
                setTransport( transport, location );

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

        /// <summary>
        ///     Override in a subclass to create the specific type of transport that is
        ///     being implemented.
        /// </summary>
        protected virtual ITransport DoCreateTransport( Uri location, Socket socket, IWireFormat wireFormat ) => new TcpTransport( location, socket, wireFormat );

        // DISCUSSION: Caching host entries may not be the best strategy when using the
        // failover protocol.  The failover protocol needs to be very dynamic when looking
        // up hostnames at runtime.  If old hostname->IP mappings are kept around, this may
        // lead to runtime failures that could have been avoided by dynamically looking up
        // the new hostname IP.
#if CACHE_HOSTENTRIES
		private static IDictionary<string, IPHostEntry> CachedIPHostEntries = new Dictionary<string, IPHostEntry>();
		private static readonly object _syncLock = new object();
#endif

        public static IPHostEntry GetIPHostEntry( String host )
        {
            IPHostEntry ipEntry;

#if CACHE_HOSTENTRIES
			string hostUpperName = host.ToUpper();

			lock (_syncLock)
			{
				if (!CachedIPHostEntries.TryGetValue(hostUpperName, out ipEntry))
				{
					try
					{
						ipEntry = Dns.GetHostEntry(hostUpperName);
						CachedIPHostEntries.Add(hostUpperName, ipEntry);
					}
					catch
					{
						ipEntry = null;
					}
				}
			}
#else
            try
            {
                ipEntry = Dns.GetHostEntry( host );
            }
            catch
            {
                ipEntry = null;
            }
#endif

            return ipEntry;
        }

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

        public static IPAddress GetIPAddress( String hostname, AddressFamily addressFamily )
        {
            IPAddress ipaddress = null;
            var hostEntry = GetIPHostEntry( hostname );

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
                    var hostEntry = GetIPHostEntry( host );

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
    }
}