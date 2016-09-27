#region Usings

using System;
using Apache.NMS;
using Apache.NMS.Policies;
using Apache.NMS.Stomp;
using Apache.NMS.Stomp.Transport;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;
using Extend;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Represents a connection with a message broker
    /// </summary>
    public class ConnectionFactory : IConnectionFactory
    {
        #region Fields

        /// <summary>
        ///     Object used to synchronize threads to create a client id generator.
        /// </summary>
        private readonly Object _syncCreateClientIdGenerator = new Object();

        /// <summary>
        ///     Stores the transport factory.
        /// </summary>
        private readonly ITransportFactory _transportFactory;

        /// <summary>
        ///     The redelivery policy.
        /// </summary>
        private IRedeliveryPolicy _redeliveryPolicy = new RedeliveryPolicy();

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the stomp connection settings.
        /// </summary>
        /// <value>The stomp connection settings.</value>
        private StompConnectionSettings StompConnectionSettings { get; }

        /// <summary>
        ///     Gets a client id generator.
        /// </summary>
        private IdGenerator ClientIdGenerator
        {
            get
            {
                if ( StompConnectionSettings.ClientIdGenerator != null )
                    return StompConnectionSettings.ClientIdGenerator;

                lock ( _syncCreateClientIdGenerator )
                {
                    if ( StompConnectionSettings.ClientIdGenerator != null )
                        return StompConnectionSettings.ClientIdGenerator;

                    return StompConnectionSettings.ClientIdGenerator = StompConnectionSettings.ClientIdPrefix.IsNotEmpty()
                        ? new IdGenerator( StompConnectionSettings.ClientIdPrefix )
                        : new IdGenerator();
                }
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionFactory" /> class.
        /// </summary>
        /// <param name="brokerUri">The broker URI.</param>
        /// <param name="stompConnectionSettings">The STOM connection settings.</param>
        public ConnectionFactory( String brokerUri, StompConnectionSettings stompConnectionSettings )
        {
            BrokerUri = URISupport.CreateCompatibleUri( brokerUri );
            StompConnectionSettings = stompConnectionSettings;
            _transportFactory = new TransportFactory( StompConnectionSettings );
        }

        #endregion

        #region Private Members

        /// <summary>
        ///     Configures the given connection.
        /// </summary>
        /// <param name="connection">The connection to configure.</param>
        private void ConfigureConnection( Connection connection )
        {
            connection.RedeliveryPolicy = _redeliveryPolicy.Clone() as IRedeliveryPolicy;
            connection.PrefetchPolicy = StompConnectionSettings.PrefetchPolicy.Clone() as PrefetchPolicy;
        }

        #endregion

        #region Implementation of IConnectionFactory

        /// <summary>
        ///     Get/or set the broker Uri.
        /// </summary>
        public Uri BrokerUri { get; set; }

        /// <summary>
        ///     Get or set the redelivery policy that new IConnection objects are assigned upon creation.
        /// </summary>
        public IRedeliveryPolicy RedeliveryPolicy
        {
            get { return _redeliveryPolicy; }
            set
            {
                if ( value != null )
                    _redeliveryPolicy = value;
                else
                    throw new ArgumentException( "Value can not be null", nameof( value ) );
            }
        }

        /// <summary>
        ///     Creates a new connection with the given user name and password
        /// </summary>
        public IConnection CreateConnection()
        {
            Connection connection = null;

            try
            {
                var transport = _transportFactory.CreateTransport( BrokerUri );
                connection = new Connection( BrokerUri, transport, ClientIdGenerator, StompConnectionSettings )
                {
                    UserName = StompConnectionSettings.UserName,
                    Password = StompConnectionSettings.Password
                };

                ConfigureConnection( connection );

                // Set the client id if set
                if ( StompConnectionSettings.ClientId.IsNotEmpty() )
                    connection.DefaultClientId = StompConnectionSettings.ClientId;

                return connection;
            } /*
            catch ( NMSException ex )
            {
                try
                {
                    connection?.Close();
                }
                catch
                {
                    // ignored
                }

                throw;
            }*/
            catch ( Exception ex )
            {
                try
                {
                    connection?.Close();
                }
                catch
                {
                    // ignored
                }

                throw NMSExceptionSupport.Create( $"Could not connect to broker URL: '{BrokerUri}'. See inner exception for details.", ex );
            }
        }

        #endregion
    }
}