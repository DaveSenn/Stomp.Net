#region Usings

using System;
using Apache.NMS.Stomp.Transport.Failover;
using Extend;
using JetBrains.Annotations;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     Transport factory, creating transports based on the given connection type.
    /// </summary>
    public class TransportFactory : ITransportFactory
    {
        #region Fields

        /// <summary>
        ///     The STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportFactory" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">stompConnectionSettings can not be null.</exception>
        /// <param name="stompConnectionSettings">Some STOMP settings.</param>
        public TransportFactory( [NotNull] StompConnectionSettings stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        #region Private Members

        /// <summary>
        ///     Create a transport factory for the scheme.
        ///     If we do not support the transport protocol, an NMSConnectionException will be thrown.
        /// </summary>
        /// <param name="location">An URI.</param>
        /// <returns>Returns a <see cref="ITransportFactory" />.</returns>
        private ITransportFactory CreateTransportFactory( Uri location )
        {
            if ( location.Scheme.IsEmpty() )
                throw new NMSConnectionException( $"Transport scheme invalid: [{location}]" );

            ITransportFactory factory;

            try
            {
                switch ( location.Scheme.ToLower() )
                {
                    case "failover":
                        factory = new FailoverTransportFactory( _stompConnectionSettings );
                        break;
                    case "tcp":
                        factory = new TcpTransportFactory( _stompConnectionSettings );
                        break;
                    case "ssl":
                        factory = new SslTransportFactory( _stompConnectionSettings );
                        break;
                    default:
                        throw new NMSConnectionException( $"The transport {location.Scheme} is not supported." );
                }
            }
            catch ( NMSConnectionException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                throw new NMSConnectionException( "Error creating transport.", ex );
            }

            if ( null == factory )
                throw new NMSConnectionException( "Unable to create a transport." );

            return factory;
        }

        #endregion

        #region Implementation of ITransportFactory

        public ITransport CompositeConnect( Uri location )
            => CreateTransportFactory( location )
                .CompositeConnect( location );

        public ITransport CreateTransport( Uri location )
            => CreateTransportFactory( location )
                .CreateTransport( location );

        #endregion
    }
}