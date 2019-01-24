#region Usings

using System;
using System.Net.Sockets;
using Extend;
using JetBrains.Annotations;
using Stomp.Net.Stomp.Protocol;
using Stomp.Net.Stomp.Transport;

#endregion

namespace Stomp.Net.Transport
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
            stompConnectionSettings.ThrowIfNull( nameof(stompConnectionSettings) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        #region Implementation of ITransportFactory

        public ITransport CreateTransport( Uri location )
        {
            var socket = Connect( location.Host, location.Port );
            socket.ReceiveBufferSize = _stompConnectionSettings.TransportSettings.ReceiveBufferSize;
            socket.SendBufferSize = _stompConnectionSettings.TransportSettings.SendBufferSize;
            socket.ReceiveTimeout = _stompConnectionSettings.TransportSettings.ReceiveTimeout;
            socket.SendTimeout = _stompConnectionSettings.TransportSettings.SendTimeout;

            var wireFormat = new StompWireFormat
            {
                SkipDestinationNameFormatting = _stompConnectionSettings.SkipDestinationNameFormatting,
                SetHostHeader = _stompConnectionSettings.SetHostHeader,
                HostHeaderOverride = _stompConnectionSettings.HostHeaderOverride
            };

            var transport = CreateTransport( location, socket, wireFormat );
            wireFormat.Transport = transport;

            if ( _stompConnectionSettings.TransportSettings.UseLogging )
                transport = new LoggingTransport( transport );

            if ( _stompConnectionSettings.TransportSettings.UseInactivityMonitor )
                transport = new InactivityMonitor( transport, wireFormat );

            transport = new MutexTransport( transport );
            transport = new ResponseCorrelator( transport );

            return transport;
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

        #region Private Members

        /// <summary>
        ///     Tries to create an open socket to the host with the given name/IP and port.
        /// </summary>
        /// <exception cref="StompConnectionException">Connection failed.</exception>
        /// <exception cref="ArgumentNullException">host can not be null.</exception>
        /// <param name="host">The host name.</param>
        /// <param name="port">The port.</param>
        /// <returns>Returns an open socket, or null if the communication has failed.</returns>
        [NotNull]
        private static Socket Connect( [NotNull] String host, Int32 port )
        {
            host.ThrowIfNull( nameof(host) );

            try
            {
                var socket = ConnectSocket( host, port );
                if ( socket != null )
                    return socket;

                throw new Exception( "General connection error." );
            }
            catch ( Exception ex )
            {
                throw new StompConnectionException( $"Error connecting to {host}:{port}.", ex );
            }
        }

        /// <summary>
        ///     Creates and connects a new socket to the given endpoint.
        /// </summary>
        /// <param name="host">The name of the host.</param>
        /// <param name="port">The endpoint port.</param>
        /// <returns>Returns the connected socket, or null if the connection failed.</returns>
        [CanBeNull]
        private static Socket ConnectSocket( [NotNull] String host, Int32 port )
        {
            try
            {
                var socket = new Socket( SocketType.Stream, ProtocolType.Tcp );
                socket.Connect( host, port );

                if ( socket.Connected )
                {
                    if ( Tracer.IsDebugEnabled )
                        Tracer.Debug( $"Socket connected {host}:{port}." );
                    return socket;
                }
            }
            catch ( Exception ex )
            {
                if ( Tracer.IsErrorEnabled )
                    Tracer.Error( $"Connect socket failed: {ex}." );
            }

            return null;
        }

        #endregion
    }
}