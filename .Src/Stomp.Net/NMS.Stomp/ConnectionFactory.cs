#region Usings

using System;
using Apache.NMS.Policies;
using Apache.NMS.Stomp.Transport;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;

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
                brokerUri = new Uri( URISupport.StripPrefix( value.OriginalString, "stomp:" ) );

                if ( !String.IsNullOrEmpty( brokerUri.Query ) && !brokerUri.OriginalString.EndsWith( ")" ) )
                {
                    // Since the Uri class will return the end of a Query string found in a Composite
                    // URI we must ensure that we trim that off before we proceed.
                    var query = brokerUri.Query.Substring( brokerUri.Query.LastIndexOf( ")" ) + 1 );

                    var properties = URISupport.ParseQuery( query );

                    var connection = URISupport.ExtractProperties( properties, "connection." );
                    var nms = URISupport.ExtractProperties( properties, "nms." );

                    if ( connection != null )
                        URISupport.SetProperties( this, connection, "connection." );

                    if ( nms != null )
                    {
                        URISupport.SetProperties( PrefetchPolicy, nms, "nms.PrefetchPolicy." );
                        URISupport.SetProperties( RedeliveryPolicy, nms, "nms.RedeliveryPolicy." );
                    }

                    brokerUri = URISupport.CreateRemainingUri( brokerUri, properties );
                }
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