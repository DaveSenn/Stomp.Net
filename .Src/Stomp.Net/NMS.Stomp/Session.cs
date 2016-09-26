

#region Usings

using System;
using System.Collections;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;
using Queue = Apache.NMS.Stomp.Commands.Queue;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Default provider of ISession
    /// </summary>
    public class Session : ISession, IDispatcher
    {
        #region Fields

        private readonly IDictionary consumers = Hashtable.Synchronized( new Hashtable() );

        private readonly SessionInfo info;

        /// <summary>
        ///     Private object used for synchronization, instead of public "this"
        /// </summary>
        private readonly Object myLock = new Object();

        private readonly IDictionary producers = Hashtable.Synchronized( new Hashtable() );
        private Boolean closed;
        private Boolean closing;
        private Int32 consumerCounter;

        private Boolean disposed;
        private Int32 nextDeliveryId;
        private Int32 producerCounter;

        #endregion

        #region Properties

        public Boolean Started
        {
            get { return Executor != null ? Executor.Running : false; }
        }

        #endregion

        #region Ctor

        public Session( Connection connection, SessionInfo info, AcknowledgementMode acknowledgementMode, Boolean dispatchAsync )
        {
            Connection = connection;
            this.info = info;
            AcknowledgementMode = acknowledgementMode;
            RequestTimeout = connection.RequestTimeout;
            DispatchAsync = dispatchAsync;

            if ( acknowledgementMode == AcknowledgementMode.Transactional )
                TransactionContext = new TransactionContext( this );
            else if ( acknowledgementMode == AcknowledgementMode.DupsOkAcknowledge )
                AcknowledgementMode = AcknowledgementMode.AutoAcknowledge;

            Executor = new SessionExecutor( this, consumers );
        }

        #endregion

        public void Dispatch( MessageDispatch dispatch )
        {
            if ( Executor != null )
            {
                if ( Tracer.IsDebugEnabled )
                    Tracer.DebugFormat( "Send Message Dispatch: ", dispatch.ToString() );
                Executor.Execute( dispatch );
            }
        }

        public void AddConsumer( MessageConsumer consumer )
        {
            if ( !closing )
            {
                // Registered with Connection before we register at the broker.
                consumers[consumer.ConsumerId] = consumer;
                Connection.addDispatcher( consumer.ConsumerId, this );
            }
        }

        public void DisposeOf( ConsumerId objectId )
        {
            Connection.removeDispatcher( objectId );
            if ( !closing )
                consumers.Remove( objectId );
        }

        public void DisposeOf( ProducerId objectId )
        {
            if ( !closing )
                producers.Remove( objectId );
        }

        public void DoSend( Message message, MessageProducer producer, TimeSpan sendTimeout )
        {
            var msg = message;

            if ( Transacted )
            {
                DoStartTransaction();
                msg.TransactionId = TransactionContext.TransactionId;
            }

            msg.RedeliveryCounter = 0;

            if ( Connection.CopyMessageOnSend )
                msg = (Message) msg.Clone();

            msg.OnSend();
            msg.ProducerId = msg.MessageId.ProducerId;

            if ( sendTimeout.TotalMilliseconds <= 0 && !msg.ResponseRequired && !Connection.AlwaysSyncSend &&
                 ( !msg.Persistent || Connection.AsyncSend || msg.TransactionId != null ) )
            {
                Connection.Oneway( msg );
            }
            else
            {
                if ( sendTimeout.TotalMilliseconds > 0 )
                    Connection.SyncRequest( msg, sendTimeout );
                else
                    Connection.SyncRequest( msg );
            }
        }

        /// <summary>
        ///     Ensures that a transaction is started
        /// </summary>
        public void DoStartTransaction()
        {
            if ( Transacted )
                TransactionContext.Begin();
        }

        public ConsumerId GetNextConsumerId()
        {
            var id = new ConsumerId();
            id.ConnectionId = info.SessionId.ConnectionId;
            id.SessionId = info.SessionId.Value;
            id.Value = Interlocked.Increment( ref consumerCounter );

            return id;
        }

        public ProducerId GetNextProducerId()
        {
            var id = new ProducerId();
            id.ConnectionId = info.SessionId.ConnectionId;
            id.SessionId = info.SessionId.Value;
            id.Value = Interlocked.Increment( ref producerCounter );

            return id;
        }

        public void RemoveConsumer( MessageConsumer consumer )
        {
            Connection.removeDispatcher( consumer.ConsumerId );
            if ( !closing )
                consumers.Remove( consumer.ConsumerId );
        }

        public void Start()
        {
            foreach ( MessageConsumer consumer in consumers.Values )
                consumer.Start();

            if ( Executor != null )
                Executor.Start();
        }

        public void Stop()
        {
            if ( Executor != null )
                Executor.Stop();
        }

        protected virtual ProducerInfo CreateProducerInfo( IDestination destination )
        {
            var answer = new ProducerInfo();
            answer.ProducerId = GetNextProducerId();
            answer.Destination = Destination.Transform( destination );

            // If the destination contained a URI query, then use it to set public
            // properties on the ProducerInfo
            var amqDestination = destination as Destination;
            if ( amqDestination != null && amqDestination.Options != null )
            {
                var options = URISupport.GetProperties( amqDestination.Options, "producer." );
                URISupport.SetProperties( answer, options );
            }

            return answer;
        }

        internal void Acknowledge()
        {
            lock ( consumers.SyncRoot )
                foreach ( MessageConsumer consumer in consumers.Values )
                    consumer.Acknowledge();
        }

        internal void ClearMessagesInProgress()
        {
            if ( Executor != null )
                Executor.ClearMessagesInProgress();

            if ( Transacted )
                TransactionContext.ResetTransactionInProgress();

            lock ( consumers.SyncRoot )
                foreach ( MessageConsumer consumer in consumers.Values )
                {
                    consumer.InProgressClearRequired();
                    ThreadPool.QueueUserWorkItem( ClearMessages, consumer );
                }
        }

        internal void Redispatch( MessageDispatchChannel channel )
        {
            var messages = channel.RemoveAll();
            Array.Reverse( messages );

            foreach ( var message in messages )
            {
                if ( Tracer.IsDebugEnabled )
                    Tracer.DebugFormat( "Resending Message Dispatch: ", message.ToString() );
                Executor.ExecuteFirst( message );
            }
        }

        internal void SendAck( MessageAck ack ) => SendAck( ack, false );

        internal void SendAck( MessageAck ack, Boolean lazy )
        {
            if ( lazy || Connection.SendAcksAsync || IsTransacted )
                Connection.Oneway( ack );
            else
                Connection.SyncRequest( ack );
        }

        private void CheckClosed()
        {
            if ( closed )
                throw new IllegalStateException( "The Session is Closed" );
        }

        private void ClearMessages( Object value )
        {
            var consumer = value as MessageConsumer;

            if ( Tracer.IsDebugEnabled )
                Tracer.Debug( "Performing Async Clear of In Progress Messages for Consumer: " + consumer.ConsumerId );

            consumer.ClearMessagesInProgress();
        }

        private Message ConfigureMessage( Message message )
        {
            message.Connection = Connection;

            if ( IsTransacted )
                message.Acknowledger += DoNothingAcknowledge;

            return message;
        }

        /// <summary>
        ///     Prevents message from throwing an exception if a client calls Acknoweldge on
        ///     a message that is part of a transaction either being produced or consumed.  The
        ///     JMS Spec indicates that users should be able to call Acknowledge with no effect
        ///     if the message is in a transaction.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="Message" />
        /// </param>
        private void DoNothingAcknowledge( Message message )
        {
        }

        ~Session()
        {
            Dispose( false );
        }

        #region Session Transaction Events

        // We delegate the events to the TransactionContext since it knows
        // what the state is at all times.

        public event SessionTxEventDelegate TransactionStartedListener
        {
            add { TransactionContext.TransactionStartedListener += value; }
            remove { TransactionContext.TransactionStartedListener += value; }
        }

        public event SessionTxEventDelegate TransactionCommittedListener
        {
            add { TransactionContext.TransactionCommittedListener += value; }
            remove { TransactionContext.TransactionCommittedListener += value; }
        }

        public event SessionTxEventDelegate TransactionRolledBackListener
        {
            add { TransactionContext.TransactionRolledBackListener += value; }
            remove { TransactionContext.TransactionRolledBackListener += value; }
        }

        #endregion

        #region Property Accessors

        /// <summary>
        ///     Sets the prefetch size, the maximum number of messages a broker will dispatch to consumers
        ///     until acknowledgements are received.
        /// </summary>
        public Int32 PrefetchSize
        {
            set { Connection.PrefetchPolicy.SetAll( value ); }
        }

        /// <summary>
        ///     Sets the maximum number of messages to keep around per consumer
        ///     in addition to the prefetch window for non-durable topics until messages
        ///     will start to be evicted for slow consumers.
        ///     Must be > 0 to enable this feature
        /// </summary>
        public Int32 MaximumPendingMessageLimit
        {
            set { Connection.PrefetchPolicy.MaximumPendingMessageLimit = value; }
        }

        /// <summary>
        ///     Enables or disables whether asynchronous dispatch should be used by the broker
        /// </summary>
        public Boolean DispatchAsync { get; set; }

        /// <summary>
        ///     Enables or disables exclusive consumers when using queues. An exclusive consumer means
        ///     only one instance of a consumer is allowed to process messages on a queue to preserve order
        /// </summary>
        public Boolean Exclusive { get; set; }

        /// <summary>
        ///     Enables or disables retroactive mode for consumers; i.e. do they go back in time or not?
        /// </summary>
        public Boolean Retroactive { get; set; }

        /// <summary>
        ///     Sets the default consumer priority for consumers
        /// </summary>
        public Byte Priority { get; set; }

        public Connection Connection { get; private set; }

        public SessionId SessionId
        {
            get { return info.SessionId; }
        }

        public TransactionContext TransactionContext { get; }

        public TimeSpan RequestTimeout { get; set; }

        public Boolean Transacted
        {
            get { return AcknowledgementMode == AcknowledgementMode.Transactional; }
        }

        public AcknowledgementMode AcknowledgementMode { get; }

        public Boolean IsClientAcknowledge
        {
            get { return AcknowledgementMode == AcknowledgementMode.ClientAcknowledge; }
        }

        public Boolean IsAutoAcknowledge
        {
            get { return AcknowledgementMode == AcknowledgementMode.AutoAcknowledge; }
        }

        public Boolean IsDupsOkAcknowledge
        {
            get { return AcknowledgementMode == AcknowledgementMode.DupsOkAcknowledge; }
        }

        public Boolean IsIndividualAcknowledge
        {
            get { return AcknowledgementMode == AcknowledgementMode.IndividualAcknowledge; }
        }

        public Boolean IsTransacted
        {
            get { return AcknowledgementMode == AcknowledgementMode.Transactional; }
        }

        public SessionExecutor Executor { get; }

        public Int64 NextDeliveryId
        {
            get { return Interlocked.Increment( ref nextDeliveryId ); }
        }

        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        public ProducerTransformerDelegate ProducerTransformer { get; set; }

        #endregion

        #region ISession Members

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected void Dispose( Boolean disposing )
        {
            if ( disposed )
                return;

            if ( disposing )
            {
                // Dispose managed code here.
            }

            try
            {
                Close();
            }
            catch
            {
                // Ignore network errors.
            }

            disposed = true;
        }

        public void Close()
        {
            lock ( myLock )
            {
                if ( closed )
                    return;

                try
                {
                    Tracer.InfoFormat( "Closing The Session with Id {0}", info.SessionId.ToString() );
                    DoClose();
                    Tracer.InfoFormat( "Closed The Session with Id {0}", info.SessionId.ToString() );
                }
                catch ( Exception ex )
                {
                    Tracer.ErrorFormat( "Error during session close: {0}", ex );
                }
                finally
                {
                    Connection = null;
                    closed = true;
                    closing = false;
                }
            }
        }

        internal void DoClose()
        {
            lock ( myLock )
            {
                if ( closed )
                    return;

                try
                {
                    closing = true;

                    // Stop all message deliveries from this Session
                    Stop();

                    lock ( consumers.SyncRoot )
                        foreach ( MessageConsumer consumer in consumers.Values )
                        {
                            consumer.FailureError = Connection.FirstFailureError;
                            consumer.DoClose();
                        }
                    consumers.Clear();

                    lock ( producers.SyncRoot )
                        foreach ( MessageProducer producer in producers.Values )
                            producer.DoClose();
                    producers.Clear();

                    // If in a transaction roll it back
                    if ( IsTransacted && TransactionContext.InTransaction )
                        try
                        {
                            TransactionContext.Rollback();
                        }
                        catch
                        {
                        }

                    Connection.RemoveSession( this );
                }
                catch ( Exception ex )
                {
                    Tracer.ErrorFormat( "Error during session close: {0}", ex );
                }
                finally
                {
                    closed = true;
                    closing = false;
                }
            }
        }

        public IMessageProducer CreateProducer() => CreateProducer( null );

        public IMessageProducer CreateProducer( IDestination destination )
        {
            var command = CreateProducerInfo( destination );
            var producerId = command.ProducerId;
            MessageProducer producer = null;

            try
            {
                producer = new MessageProducer( this, command );
                producer.ProducerTransformer = ProducerTransformer;
                producers[producerId] = producer;
            }
            catch ( Exception )
            {
                if ( producer != null )
                    producer.Close();

                throw;
            }

            return producer;
        }

        public IMessageConsumer CreateConsumer( IDestination destination ) => CreateConsumer( destination, null, false );

        public IMessageConsumer CreateConsumer( IDestination destination, String selector ) => CreateConsumer( destination, selector, false );

        public IMessageConsumer CreateConsumer( IDestination destination, String selector, Boolean noLocal )
        {
            if ( destination == null )
                throw new InvalidDestinationException( "Cannot create a Consumer with a Null destination" );

            var prefetchSize = Connection.PrefetchPolicy.DurableTopicPrefetch;

            if ( destination.IsTopic )
                prefetchSize = Connection.PrefetchPolicy.TopicPrefetch;
            else if ( destination.IsQueue )
                prefetchSize = Connection.PrefetchPolicy.QueuePrefetch;

            MessageConsumer consumer = null;

            try
            {
                var dest = destination as Destination;
                consumer = new MessageConsumer( this, GetNextConsumerId(), dest, null, selector, prefetchSize, noLocal );
                consumer.ConsumerTransformer = ConsumerTransformer;
                AddConsumer( consumer );

                // lets register the consumer first in case we start dispatching messages immediately
                Connection.SyncRequest( consumer.ConsumerInfo );

                if ( Started )
                    consumer.Start();
            }
            catch ( Exception )
            {
                if ( consumer != null )
                {
                    RemoveConsumer( consumer );
                    consumer.Close();
                }

                throw;
            }

            return consumer;
        }

        public IMessageConsumer CreateDurableConsumer( ITopic destination, String name, String selector, Boolean noLocal )
        {
            if ( destination == null )
                throw new InvalidDestinationException( "Cannot create a Consumer with a Null destination" );

            MessageConsumer consumer = null;

            try
            {
                var dest = destination as Destination;
                consumer = new MessageConsumer( this, GetNextConsumerId(), dest, name, selector, Connection.PrefetchPolicy.DurableTopicPrefetch, noLocal );
                consumer.ConsumerTransformer = ConsumerTransformer;
                AddConsumer( consumer );
                Connection.SyncRequest( consumer.ConsumerInfo );

                if ( Started )
                    consumer.Start();
            }
            catch ( Exception )
            {
                if ( consumer != null )
                {
                    RemoveConsumer( consumer );
                    consumer.Close();
                }

                throw;
            }

            return consumer;
        }

        public void DeleteDurableConsumer( String name )
        {
            var command = new RemoveSubscriptionInfo();
            command.ConnectionId = Connection.ConnectionId;
            command.ClientId = Connection.ClientId;
            command.SubscriptionName = name;
            Connection.SyncRequest( command );
        }

        public IQueueBrowser CreateBrowser( IQueue queue )
        {
            throw new NotSupportedException( "Not supported with Stomp Protocol" );
        }

        public IQueueBrowser CreateBrowser( IQueue queue, String selector )
        {
            throw new NotSupportedException( "Not supported with Stomp Protocol" );
        }

        public IQueue GetQueue( String name ) => new Queue( name );

        public ITopic GetTopic( String name ) => new Topic( name );

        public ITemporaryQueue CreateTemporaryQueue()
        {
            var answer = new TempQueue( Connection.CreateTemporaryDestinationName() );
            return answer;
        }

        public ITemporaryTopic CreateTemporaryTopic()
        {
            var answer = new TempTopic( Connection.CreateTemporaryDestinationName() );
            return answer;
        }

        /// <summary>
        ///     Delete a destination (Queue, Topic, Temp Queue, Temp Topic).
        /// </summary>
        public void DeleteDestination( IDestination destination )
        {
            throw new NotSupportedException( "Stomp Cannot delete Destinations" );
        }

        public IMessage CreateMessage()
        {
            var answer = new Message();
            return ConfigureMessage( answer );
        }

        public ITextMessage CreateTextMessage()
        {
            var answer = new TextMessage();
            return ConfigureMessage( answer ) as ITextMessage;
        }

        public ITextMessage CreateTextMessage( String text )
        {
            var answer = new TextMessage( text );
            return ConfigureMessage( answer ) as ITextMessage;
        }

        public IMapMessage CreateMapMessage()
        {
            var answer = new MapMessage();
            return ConfigureMessage( answer ) as IMapMessage;
        }

        public IBytesMessage CreateBytesMessage() => ConfigureMessage( new BytesMessage() ) as IBytesMessage;

        public IBytesMessage CreateBytesMessage( Byte[] body )
        {
            var answer = new BytesMessage();
            answer.Content = body;
            return ConfigureMessage( answer ) as IBytesMessage;
        }

        public IStreamMessage CreateStreamMessage()
        {
            throw new NotSupportedException( "No Object Message in Stomp" );
        }

        public IObjectMessage CreateObjectMessage( Object body )
        {
            throw new NotSupportedException( "No Object Message in Stomp" );
        }

        public void Commit()
        {
            if ( !Transacted )
                throw new InvalidOperationException(
                    "You cannot perform a Commit() on a non-transacted session. Acknowlegement mode is: "
                    + AcknowledgementMode );

            TransactionContext.Commit();
        }

        public void Rollback()
        {
            if ( !Transacted )
                throw new InvalidOperationException(
                    "You cannot perform a Commit() on a non-transacted session. Acknowlegement mode is: "
                    + AcknowledgementMode );

            TransactionContext.Rollback();
        }

        public void Recover()
        {
            CheckClosed();

            if ( AcknowledgementMode == AcknowledgementMode.Transactional )
                throw new IllegalStateException( "Cannot Recover a Transacted Session" );

            lock ( consumers.SyncRoot )
                foreach ( MessageConsumer consumer in consumers.Values )
                    consumer.Rollback();
        }

        #endregion
    }
}