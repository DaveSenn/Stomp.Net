#region Usings

using System;
using System.Net.Sockets;
using Extend;
using JetBrains.Annotations;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    /// <summary>
    /// SSL transport factory.
    /// </summary>
    public class SslTransportFactory : TcpTransportFactory
    {
        #region Fields

        /// <summary>
        ///     Stores the STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="SslTransportFactory" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">stompConnectionSettings can not be null.</exception>
        /// <param name="stompConnectionSettings">Some STOMP settings.</param>
        public SslTransportFactory( [NotNull] StompConnectionSettings stompConnectionSettings )
            : base( stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        protected override ITransport CreateTransport( Uri location, Socket socket, IWireFormat wireFormat )
        {
            var transport = new SslTransport( location, socket, wireFormat )
            {
                ClientCertSubject = _stompConnectionSettings.TransportSettings.SslSettings.ClientCertSubject,
                ClientCertFilename = _stompConnectionSettings.TransportSettings.SslSettings.ClientCertFilename,
                ClientCertPassword = _stompConnectionSettings.TransportSettings.SslSettings.ClientCertPassword,
                ServerName = _stompConnectionSettings.TransportSettings.SslSettings.ServerName,
                KeyStoreLocation = _stompConnectionSettings.TransportSettings.SslSettings.KeyStoreLocation,
                KeyStoreName = _stompConnectionSettings.TransportSettings.SslSettings.KeyStoreName,
                AcceptInvalidBrokerCert = _stompConnectionSettings.TransportSettings.SslSettings.AcceptInvalidBrokerCert
            };

            return transport;
        }
    }
}