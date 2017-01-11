#region Usings

using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Extend;
using JetBrains.Annotations;
using Stomp.Net.Stomp.Transport;

#endregion

namespace Stomp.Net.Transport
{
    /// <summary>
    ///     SSL transport.
    /// </summary>
    public class SslTransport : TcpTransport
    {
        #region Fields

        /// <summary>
        ///     Stores the STOMP connections settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        /// <summary>
        ///     The SSL stream.
        /// </summary>
        private SslStream _sslStream;

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TcpTransport" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">stompConnectionSettings can not be null.</exception>
        /// <param name="location">The URI.</param>
        /// <param name="socket">The socket to use.</param>
        /// <param name="wireFormat">A <see cref="IWireFormat" />.</param>
        /// <param name="stompConnectionSettings">Some STOMP connection settings.</param>
        public SslTransport( Uri location, Socket socket, IWireFormat wireFormat, [NotNull] StompConnectionSettings stompConnectionSettings )
            : base( location, socket, wireFormat )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        #region Protected Members

        /// <summary>
        ///     Creates a stream for the transport socket.
        /// </summary>
        /// <returns>Returns the newly created stream.</returns>
        protected override Stream CreateSocketStream()
        {
            if ( _sslStream != null )
                return _sslStream;

            _sslStream = new SslStream( new NetworkStream( Socket ), false, ValidateServerCertificate, SelectLocalCertificate );

            try
            {
                var remoteCertName = _stompConnectionSettings.TransportSettings.SslSettings.ServerName ?? RemoteAddress.Host;
                _sslStream.AuthenticateAsClient( remoteCertName, LoadCertificates(), SslProtocols.Default, false );
            }
            catch ( Exception ex )
            {
                Tracer.Error( "Authentication failed - closing the connection." );
                Tracer.ErrorFormat( "Exception: {0}", ex.ToString() );
                throw;
            }

            return _sslStream;
        }

        #endregion

        #region Private Members

        private X509Certificate2Collection LoadCertificates()
        {
            var collection = new X509Certificate2Collection();

            if ( _stompConnectionSettings.TransportSettings.SslSettings.ClientCertFilename.IsNotEmpty() )
            {
                var certificate = new X509Certificate2( _stompConnectionSettings.TransportSettings.SslSettings.ClientCertFilename,
                                                        _stompConnectionSettings.TransportSettings.SslSettings.ClientCertPassword );

                collection.Add( certificate );
            }
            else
            {
                var name = _stompConnectionSettings.TransportSettings.SslSettings.KeyStoreName.IsEmpty()
                    ? StoreName.My.ToString()
                    : _stompConnectionSettings.TransportSettings.SslSettings.KeyStoreName;

                var location = StoreLocation.CurrentUser;

                if ( _stompConnectionSettings.TransportSettings.SslSettings.KeyStoreLocation.IsNotEmpty() )
                    if ( String.Compare( _stompConnectionSettings.TransportSettings.SslSettings.KeyStoreLocation, "CurrentUser", StringComparison.OrdinalIgnoreCase ) == 0 )
                        location = StoreLocation.CurrentUser;
                    else if ( String.Compare( _stompConnectionSettings.TransportSettings.SslSettings.KeyStoreLocation, "LocalMachine", StringComparison.OrdinalIgnoreCase ) == 0 )
                        location = StoreLocation.LocalMachine;
                    else
                        throw new StompException( "Invalid StoreLocation given on URI" );

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
            if ( localCertificates.Count == 1 )
                return localCertificates[0];
            if ( localCertificates.Count <= 1 || _stompConnectionSettings.TransportSettings.SslSettings.ClientCertSubject == null )
                return null;

            return localCertificates
                .Cast<X509Certificate2>()
                .FirstOrDefault(
                    certificate =>
                            String.Compare( certificate.Subject, _stompConnectionSettings.TransportSettings.SslSettings.ClientCertSubject, StringComparison.OrdinalIgnoreCase ) == 0 );
        }

        private Boolean ValidateServerCertificate( Object sender,
                                                   X509Certificate certificate,
                                                   X509Chain chain,
                                                   SslPolicyErrors sslPolicyErrors )
        {
            switch ( sslPolicyErrors )
            {
                case SslPolicyErrors.None:
                    return true;
                case SslPolicyErrors.RemoteCertificateChainErrors:
                    Tracer.Error( "Chain Status errors: " );
                    foreach ( var status in chain.ChainStatus )
                    {
                        Tracer.Error( "Chain Status error: " + status.Status );
                        Tracer.Error( "Chain Status information: " + status.StatusInformation );
                    }
                    break;
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    Tracer.Error( "Mismatch between Remote Cert Name." );
                    break;
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    Tracer.Error( "The Remote Certificate was not Available." );
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( sslPolicyErrors ), sslPolicyErrors, $"Policy '{sslPolicyErrors}' is not supported." );
            }

            // Configuration may or may not allow us to connect with an invalid broker cert.
            return _stompConnectionSettings.TransportSettings.SslSettings.AcceptInvalidBrokerCert;
        }

        #endregion
    }
}