#region Usings

using System;
using Extend;
using JetBrains.Annotations;
using Stomp.Net.Transport;

#endregion

namespace Stomp.Net.Stomp.Transport
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
            stompConnectionSettings.ThrowIfNull( nameof(stompConnectionSettings) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        #region Implementation of ITransportFactory

        public ITransport CreateTransport( Uri location )
            => CreateTransportFactory( location )
                .CreateTransport( location );

        #endregion

        #region Private Members

        /// <summary>
        ///     Create a transport factory for the scheme.
        ///     If we do not support the transport protocol, an StompConnectionException will be thrown.
        /// </summary>
        /// <param name="location">An URI.</param>
        /// <returns>Returns a <see cref="ITransportFactory" />.</returns>
        private ITransportFactory CreateTransportFactory( Uri location )
        {
            if ( location.Scheme.IsEmpty() )
                throw new StompConnectionException( $"Transport scheme invalid: [{location}]" );

            ITransportFactory factory;

            try
            {
                factory = location.Scheme.ToLower() switch
                {
                    "tcp" => new TcpTransportFactory( _stompConnectionSettings ),
                    "ssl" => new SslTransportFactory( _stompConnectionSettings ),
                    _ => throw new StompConnectionException( $"The transport {location.Scheme} is not supported." )
                };
            }
            catch ( StompConnectionException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                throw new StompConnectionException( "Error creating transport.", ex );
            }

            if ( null == factory )
                throw new StompConnectionException( "Unable to create a transport." );

            return factory;
        }

        #endregion
    }
}