

#region Usings

using System;
using System.Net.Sockets;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    public class SslTransportFactory : TcpTransportFactory
    {
        #region Fields

        #endregion

        #region Properties

        public String ServerName { get; set; }

        public String ClientCertSubject { get; set; }

        public String ClientCertFilename { get; set; }

        public String ClientCertPassword { get; set; }

        public Boolean AcceptInvalidBrokerCert { get; set; } = false;

        public String KeyStoreName { get; set; }

        public String KeyStoreLocation { get; set; }

        #endregion

        protected override ITransport DoCreateTransport( Uri location, Socket socket, IWireFormat wireFormat )
        {
            Tracer.Debug( "Creating new instance of the SSL Transport." );
#if !NETCF
            var transport = new SslTransport( location, socket, wireFormat );

            transport.ClientCertSubject = ClientCertSubject;
            transport.ClientCertFilename = ClientCertFilename;
            transport.ClientCertPassword = ClientCertPassword;
            transport.ServerName = ServerName;
            transport.KeyStoreLocation = KeyStoreLocation;
            transport.KeyStoreName = KeyStoreName;
            transport.AcceptInvalidBrokerCert = AcceptInvalidBrokerCert;

            return transport;
#else
            throw new NotSupportedException("SslTransport not implemented on the .NET Compact Framework.");
#endif
        }
    }
}