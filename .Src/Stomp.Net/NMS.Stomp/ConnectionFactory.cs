#region Usings

using System;
using System.Collections.Specialized;
using System.Linq;
using Apache.NMS.Policies;
using Apache.NMS.Stomp.Transport;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;
using Extend;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Represents a connection with a message broker
    /// </summary>
    public class ConnectionFactory : IConnectionFactory
    {
        #region Fields

        private Uri brokerUri;
        private IdGenerator clientIdGenerator;

        private IRedeliveryPolicy redeliveryPolicy = new RedeliveryPolicy();
        private TimeSpan requestTimeout = NMSConstants.defaultRequestTimeout;

        #endregion

        #region Properties

        public String UserName { get; set; }

        public String Password { get; set; }

        public String ClientId { get; set; }

        public String ClientIdPrefix { get; set; }

        public Boolean CopyMessageOnSend { get; set; } = true;

        public Boolean AlwaysSyncSend { get; set; }

        public Boolean SendAcksAsync { get; set; } = true;

        public Boolean AsyncSend { get; set; }

        public String AckMode
        {
            set { AcknowledgementMode = NMSConvert.ToAcknowledgementMode( value ); }
        }

        public Boolean DispatchAsync { get; set; } = true;

        public Int32 RequestTimeout
        {
            get { return (Int32) requestTimeout.TotalMilliseconds; }
            set { requestTimeout = TimeSpan.FromMilliseconds( value ); }
        }

        public AcknowledgementMode AcknowledgementMode { get; set; } = AcknowledgementMode.AutoAcknowledge;

        public PrefetchPolicy PrefetchPolicy { get; set; } = new PrefetchPolicy();

        public IdGenerator ClientIdGenerator
        {
            set { clientIdGenerator = value; }
            get
            {
                lock ( this )
                {
                    if ( clientIdGenerator == null )
                        if ( ClientIdPrefix != null )
                            clientIdGenerator = new IdGenerator( ClientIdPrefix );
                        else
                            clientIdGenerator = new IdGenerator();

                    return clientIdGenerator;
                }
            }
        }

        #endregion

        #region Ctor

        static ConnectionFactory()
        {
            TransportFactory.OnException += ExceptionHandler;
        }

        public ConnectionFactory( String brokerUri, String clientId = null )
        {
            BrokerUri = URISupport.CreateCompatibleUri( brokerUri );
            ClientId = clientId;
        }

        #endregion

        /// <summary>
        ///     Get/or set the broker Uri.
        /// </summary>
        public Uri BrokerUri
        {
            get { return brokerUri; }
            set
            {
                // brokerUri = new Uri( value.OriginalString );
                brokerUri = value;

                // Check for query parameters
                if ( !brokerUri.Query.IsNotEmpty() || brokerUri.OriginalString.EndsWith( ")", StringComparison.Ordinal ) )
                    return;

                // Since the Uri class will return the end of a Query string found in a Composite
                // URI we must ensure that we trim that off before we proceed.
                // Call does not change the given URL
                // var query = brokerUri.Query.Substring( brokerUri.Query.LastIndexOf( ")", StringComparison.Ordinal ) + 1 );

                var queryParameters = URISupport.ParseQuery(brokerUri.Query);
                // Remove the connection properties ...TODO: why?
                var connectionProperties = URISupport.ExtractProperties( queryParameters, "connection." );
                // Remove the NMS properties ...TODO: why?
                var nmsProperties = URISupport.ExtractProperties( queryParameters, "nms." );

                if ( connectionProperties.Any() )
                    URISupport.SetProperties( this, connectionProperties, "connection." );

                if ( nmsProperties.Any())
                {
                    URISupport.SetProperties( PrefetchPolicy, nmsProperties, "nms.PrefetchPolicy." );
                    URISupport.SetProperties( RedeliveryPolicy, nmsProperties, "nms.RedeliveryPolicy." );
                }

                brokerUri = URISupport.CreateRemainingUri( brokerUri, queryParameters );
            }
        }

        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        public IConnection CreateConnection() 
            => CreateConnection( UserName, Password );

        public IConnection CreateConnection( String userName, String password )
        {
            Connection connection = null;

            try
            {
                Tracer.InfoFormat( "Connecting to: {0}", brokerUri.ToString() );

                var transport = TransportFactory.CreateTransport( brokerUri );

                connection = new Connection( brokerUri, transport, ClientIdGenerator )
                {
                    UserName = userName,
                    Password = password
                };

                ConfigureConnection( connection );

                if ( ClientId != null )
                    connection.DefaultClientId = ClientId;

                return connection;
            }
            catch ( NMSException e )
            {
                try
                {
                    connection.Close();
                }
                catch
                {
                }

                throw;
            }
            catch ( Exception e )
            {
                try
                {
                    connection.Close();
                }
                catch
                {
                }

                throw NMSExceptionSupport.Create( "Could not connect to broker URL: " + brokerUri + ". Reason: " + e.Message, e );
            }
        }

        public ProducerTransformerDelegate ProducerTransformer { get; set; }

        public IRedeliveryPolicy RedeliveryPolicy
        {
            get { return redeliveryPolicy; }
            set
            {
                if ( value != null )
                    redeliveryPolicy = value;
            }
        }

        public event ExceptionListener OnException
        {
            add { onException += value; }
            remove
            {
                if ( onException != null )
                    onException -= value;
            }
        }

        protected virtual void ConfigureConnection( Connection connection )
        {
            connection.AsyncSend = AsyncSend;
            connection.CopyMessageOnSend = CopyMessageOnSend;
            connection.AlwaysSyncSend = AlwaysSyncSend;
            connection.SendAcksAsync = SendAcksAsync;
            connection.DispatchAsync = DispatchAsync;
            connection.AcknowledgementMode = AcknowledgementMode;
            connection.RequestTimeout = requestTimeout;
            connection.RedeliveryPolicy = redeliveryPolicy.Clone() as IRedeliveryPolicy;
            connection.PrefetchPolicy = PrefetchPolicy.Clone() as PrefetchPolicy;
            connection.ConsumerTransformer = ConsumerTransformer;
            connection.ProducerTransformer = ProducerTransformer;
        }

        protected static void ExceptionHandler( Exception ex )
        {
            if ( onException != null )
                onException( ex );
        }

        private static event ExceptionListener onException;
    }
}