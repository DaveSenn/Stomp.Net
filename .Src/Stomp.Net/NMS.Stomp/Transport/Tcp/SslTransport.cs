#if !NETCF

#region Usings

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    public class SslTransport : TcpTransport
    {
        #region Fields

        private SslStream sslStream;

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates the name of the Server's Certificate.  By default the Host name
        ///     of the remote server is used, however if this doesn't match the name of the
        ///     Server's certificate then this option can be set to override the default.
        /// </summary>
        public String ServerName { get; set; }

        public String ClientCertSubject { get; set; }

        /// <summary>
        ///     Indicates the location of the Client Certificate to use when the Broker
        ///     is configured for Client Auth (not common).  The SslTransport will supply
        ///     this certificate to the SslStream via the SelectLocalCertificate method.
        /// </summary>
        public String ClientCertFilename { get; set; }

        /// <summary>
        ///     Password for the Client Certificate specified via configuration.
        /// </summary>
        public String ClientCertPassword { get; set; }

        /// <summary>
        ///     Indicates if the SslTransport should ignore any errors in the supplied Broker
        ///     certificate and connect anyway, this is useful in testing with a default AMQ
        ///     broker certificate that is self signed.
        /// </summary>
        public Boolean AcceptInvalidBrokerCert { get; set; } = false;

        public String KeyStoreName { get; set; }

        public String KeyStoreLocation { get; set; }

        #endregion

        #region Ctor

        public SslTransport( Uri location, Socket socket, IWireFormat wireFormat )
            :
            base( location, socket, wireFormat )
        {
        }

        #endregion

        protected override Stream CreateSocketStream()
        {
            if ( sslStream != null )
                return sslStream;

            sslStream = new SslStream(
                new NetworkStream( Socket ),
                false,
                ValidateServerCertificate,
                SelectLocalCertificate );

            try
            {
                var remoteCertName = ServerName ?? RemoteAddress.Host;
                Tracer.Debug( "Authorizing as Client for Server: " + remoteCertName );
                sslStream.AuthenticateAsClient( remoteCertName, LoadCertificates(), SslProtocols.Default, false );
                Tracer.Debug( "Server is Authenticated = " + sslStream.IsAuthenticated );
                Tracer.Debug( "Server is Encrypted = " + sslStream.IsEncrypted );
            }
            catch ( Exception e )
            {
                Tracer.ErrorFormat( "Exception: {0}", e.Message );
                if ( e.InnerException != null )
                    Tracer.ErrorFormat( "Inner exception: {0}", e.InnerException.Message );
                Tracer.Error( "Authentication failed - closing the connection." );

                throw e;
            }

            return sslStream;
        }

        private X509Certificate2Collection LoadCertificates()
        {
            var collection = new X509Certificate2Collection();

            if ( !String.IsNullOrEmpty( ClientCertFilename ) )
            {
                Tracer.Debug( "Attempting to load Client Certificate from file := " + ClientCertFilename );
                var certificate = new X509Certificate2( ClientCertFilename, ClientCertPassword );
                Tracer.Debug( "Loaded Client Certificate := " + certificate );

                collection.Add( certificate );
            }
            else
            {
                var name = String.IsNullOrEmpty( KeyStoreName ) ? StoreName.My.ToString() : KeyStoreName;

                var location = StoreLocation.CurrentUser;

                if ( !String.IsNullOrEmpty( KeyStoreLocation ) )
                    if ( String.Compare( KeyStoreLocation, "CurrentUser", true ) == 0 )
                        location = StoreLocation.CurrentUser;
                    else if ( String.Compare( KeyStoreLocation, "LocalMachine", true ) == 0 )
                        location = StoreLocation.LocalMachine;
                    else
                        throw new NMSException( "Invalid StoreLocation given on URI" );

                var store = new X509Store( name, location );
                store.Open( OpenFlags.ReadOnly );
                collection = store.Certificates;
                store.Close();
            }

            return collection;
        }

        private X509Certificate SelectLocalCertificate( Object sender,
                                                        String targetHost,
                                                        X509CertificateCollection localCertificates,
                                                        X509Certificate remoteCertificate,
                                                        String[] acceptableIssuers )
        {
            Tracer.DebugFormat( "Client is selecting a local certificate from {0} possibilities.", localCertificates.Count );

            if ( localCertificates.Count == 1 )
            {
                Tracer.Debug( "Client has selected certificate with Subject = " + localCertificates[0].Subject );
                return localCertificates[0];
            }
            if ( localCertificates.Count > 1 && ClientCertSubject != null )
                foreach ( X509Certificate2 certificate in localCertificates )
                {
                    Tracer.Debug( "Checking Client Certificate := " + certificate );
                    if ( String.Compare( certificate.Subject, ClientCertSubject, true ) == 0 )
                    {
                        Tracer.Debug( "Client has selected certificate with Subject = " + certificate.Subject );
                        return certificate;
                    }
                }

            Tracer.Debug( "Client did not select a Certificate, returning null." );
            return null;
        }

        private Boolean ValidateServerCertificate( Object sender,
                                                   X509Certificate certificate,
                                                   X509Chain chain,
                                                   SslPolicyErrors sslPolicyErrors )
        {
            Tracer.DebugFormat( "ValidateServerCertificate: Issued By {0}", certificate.Issuer );
            if ( sslPolicyErrors == SslPolicyErrors.None )
                return true;

            Tracer.WarnFormat( "Certificate error: {0}", sslPolicyErrors.ToString() );
            if ( sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors )
            {
                Tracer.Error( "Chain Status errors: " );
                foreach ( var status in chain.ChainStatus )
                {
                    Tracer.Error( "*** Chain Status error: " + status.Status );
                    Tracer.Error( "*** Chain Status information: " + status.StatusInformation );
                }
            }
            else if ( sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch )
            {
                Tracer.Error( "Mismatch between Remote Cert Name." );
            }
            else if ( sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable )
            {
                Tracer.Error( "The Remote Certificate was not Available." );
            }

            // Configuration may or may not allow us to connect with an invalid broker cert.
            return AcceptInvalidBrokerCert;
        }
        
    }
}

#endif