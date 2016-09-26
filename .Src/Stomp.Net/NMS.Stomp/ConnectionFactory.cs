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

        private readonly Object _syncRoot = new Object();

        private IdGenerator _clientIdGenerator;
        private IRedeliveryPolicy _redeliveryPolicy = new RedeliveryPolicy();

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

        /*
        TODO: Check if target of reflection => Query string?
        public Int32 RequestTimeout
        {
            get { return (Int32) requestTimeout.TotalMilliseconds; }
            set { requestTimeout = TimeSpan.FromMilliseconds( value ); }
        }
        */
        public TimeSpan RequestTimeout { get; set; } = NMSConstants.defaultRequestTimeout;

        public AcknowledgementMode AcknowledgementMode { get; set; } = AcknowledgementMode.AutoAcknowledge;

        public PrefetchPolicy PrefetchPolicy { get; set; } = new PrefetchPolicy();

        public IdGenerator ClientIdGenerator
        {
            get
            {
                if ( _clientIdGenerator == null )
                    lock ( _syncRoot )
                    {
                        if ( _clientIdGenerator != null )
                            return _clientIdGenerator;

                        _clientIdGenerator = ClientIdPrefix != null ? new IdGenerator( ClientIdPrefix ) : new IdGenerator();
                    }
                return _clientIdGenerator;
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
        public Uri BrokerUri { get; set; }

        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        public IConnection CreateConnection()
            => CreateConnection( UserName, Password );

        public IConnection CreateConnection( String userName, String password )
        {
            Connection connection = null;

            try
            {
                Tracer.InfoFormat( "Connecting to: {0}", BrokerUri.ToString() );

                var transport = TransportFactory.CreateTransport( BrokerUri );

                connection = new Connection( BrokerUri, transport, ClientIdGenerator )
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

                throw NMSExceptionSupport.Create( "Could not connect to broker URL: " + BrokerUri + ". Reason: " + e.Message, e );
            }
        }

        public ProducerTransformerDelegate ProducerTransformer { get; set; }

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
            connection.RequestTimeout = RequestTimeout;
            connection.RedeliveryPolicy = _redeliveryPolicy.Clone() as IRedeliveryPolicy;
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