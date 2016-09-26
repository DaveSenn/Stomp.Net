#region Usings

using System;
using System.Net.Sockets;
using JetBrains.Annotations;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    public class SslTransportFactory : TcpTransportFactory
    {
        #region Properties

        public String ServerName { get; set; }

        public String ClientCertSubject { get; set; }

        public String ClientCertFilename { get; set; }

        public String ClientCertPassword { get; set; }

        public Boolean AcceptInvalidBrokerCert { get; set; } = false;

        public String KeyStoreName { get; set; }

        public String KeyStoreLocation { get; set; }

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
        }

        #endregion

        
        protected override ITransport DoCreateTransport( Uri location, Socket socket, IWireFormat wireFormat )
        {
            var transport = new SslTransport( location, socket, wireFormat )
            {
                ClientCertSubject = ClientCertSubject,
                ClientCertFilename = ClientCertFilename,
                ClientCertPassword = ClientCertPassword,
                ServerName = ServerName,
                KeyStoreLocation = KeyStoreLocation,
                KeyStoreName = KeyStoreName,
                AcceptInvalidBrokerCert = AcceptInvalidBrokerCert
            };

            return transport;
        }
    }
}