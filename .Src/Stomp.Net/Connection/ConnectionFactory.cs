#region Usings

using System;
using Apache.NMS.Policies;
using Apache.NMS.Stomp.Transport;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;
using Extend;
using Stomp.Net;
using Apache.NMS;
using Apache.NMS.Stomp;

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
                if ( StompConnectionSettings.ClientIdGenerator == null )
                    lock ( _syncCreateClientIdGenerator )
                    {
                        if ( StompConnectionSettings.ClientIdGenerator != null )
                            return StompConnectionSettings.ClientIdGenerator;

                        StompConnectionSettings.ClientIdGenerator = StompConnectionSettings.ClientIdPrefix.IsNotEmpty()
                            ? new IdGenerator( StompConnectionSettings.ClientIdPrefix )
                            : new IdGenerator();
                    }
                return StompConnectionSettings.ClientIdGenerator;
            }
        }

        /// <summary>
        ///     Gets the acknowledge mode.
        /// </summary>
        private AcknowledgementMode AcknowledgementMode => StompConnectionSettings.AcknowledgementMode ?? StompNetConfiguration.DefaultAcknowledgementMode;

        /// <summary>
        ///     Gets the request timeout.
        /// </summary>
        private TimeSpan RequestTimeout => StompConnectionSettings.RequestTimeout ?? StompNetConfiguration.RequestTimeout;

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionFactory" /> class.
        /// </summary>
        static ConnectionFactory()
        {
            TransportFactory.OnException += ExceptionHandler;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionFactory" /> class.
        /// </summary>
        /// <param name="brokerUri">The broker URI.</param>
        /// <param name="stompConnectionSettings">The STOM connection settings.</param>
        public ConnectionFactory( String brokerUri, StompConnectionSettings stompConnectionSettings )
        {
            BrokerUri = URISupport.CreateCompatibleUri( brokerUri );
            StompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Event fired on exception.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private static event ExceptionListener _onException;

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
                Tracer.InfoFormat( "Connecting to: {0}", BrokerUri.ToString() );

                var transport = TransportFactory.CreateTransport( BrokerUri );

                connection = new Connection( BrokerUri, transport, ClientIdGenerator )
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

        /// <summary>
        ///     Event fired on exception.
        /// </summary>
        public event ExceptionListener OnException
        {
            add { _onException += value; }
            remove
            {
                if ( _onException != null )
                    _onException -= value;
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        ///     Configures the given connection.
        /// </summary>
        /// <param name="connection">The connection to configure.</param>
        private void ConfigureConnection( Connection connection )
        {
            connection.AsyncSend = StompConnectionSettings.AsyncSend;
            connection.CopyMessageOnSend = StompConnectionSettings.CopyMessageOnSend;
            connection.AlwaysSyncSend = StompConnectionSettings.AlwaysSyncSend;
            connection.SendAcksAsync = StompConnectionSettings.SendAcksAsync;
            connection.DispatchAsync = StompConnectionSettings.DispatchAsync;
            connection.AcknowledgementMode = AcknowledgementMode;
            connection.RequestTimeout = RequestTimeout;
            connection.RedeliveryPolicy = _redeliveryPolicy.Clone() as IRedeliveryPolicy;
            connection.PrefetchPolicy = StompConnectionSettings.PrefetchPolicy.Clone() as PrefetchPolicy;
        }

        /// <summary>
        ///     Publishes the <see cref="OnException" /> event.
        /// </summary>
        /// <param name="ex">The exception.</param>
        private static void ExceptionHandler( Exception ex )
            => _onException?.Invoke( ex );

        #endregion
    }
}