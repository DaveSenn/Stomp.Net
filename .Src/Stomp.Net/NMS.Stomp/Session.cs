#region Usings

using System;
using System.Collections;
using System.Threading;
using Extend;
using JetBrains.Annotations;
using Stomp.Net.Messaging;
using Stomp.Net.Stomp.Commands;
using Queue = Stomp.Net.Stomp.Commands.Queue;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Default provider of ISession
    /// </summary>
    public class Session : ISession, IDispatcher
    {
        #region Fields

        private readonly IDictionary _consumers = Hashtable.Synchronized( new Hashtable() );

        private readonly SessionInfo _info;

        /// <summary>
        ///     Private object used for synchronization, instead of public "this"
        /// </summary>
        private readonly Object _myLock = new Object();

        private readonly IDictionary _producers = Hashtable.Synchronized( new Hashtable() );

        /// <summary>
        ///     Stores the STOMP connections settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        private Boolean _closed;
        private Boolean _closing;
        private Int32 _consumerCounter;

        private Boolean _disposed;
        private Int32 _nextDeliveryId;
        private Int32 _producerCounter;

        #endregion

        #region Properties

        public Boolean Started => Executor != null && Executor.Running;

        #endregion

        #region Ctor

        public Session( Connection connection, SessionInfo info, AcknowledgementMode acknowledgementMode, [NotNull] StompConnectionSettings stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );
            _stompConnectionSettings = stompConnectionSettings;

            Connection = connection;
            _info = info;
            AcknowledgementMode = acknowledgementMode;

            if ( acknowledgementMode == AcknowledgementMode.Transactional )
                TransactionContext = new TransactionContext( this );
            else if ( acknowledgementMode == AcknowledgementMode.DupsOkAcknowledge )
                AcknowledgementMode = AcknowledgementMode.AutoAcknowledge;

            Executor = new SessionExecutor( this, _consumers );
        }

        #endregion

        public void Dispatch( MessageDispatch dispatch )
            => Executor?.Execute( dispatch );

        public void DisposeOf( ConsumerId objectId )
        {
            Connection.RemoveDispatcher( objectId );
            if ( !_closing )
                _consumers.Remove( objectId );
        }

        public void DisposeOf( ProducerId objectId )
        {
            if ( !_closing )
                _producers.Remove( objectId );
        }

        public void DoSend( Message message, TimeSpan sendTimeout )
        {
            var msg = message;

            if ( Transacted )
            {
                DoStartTransaction();
                msg.TransactionId = TransactionContext.TransactionId;
            }

            msg.RedeliveryCounter = 0;

            if ( _stompConnectionSettings.CopyMessageOnSend )
                msg = (Message) msg.Clone();

            msg.OnSend();
            msg.ProducerId = msg.MessageId.ProducerId;

            if ( sendTimeout.TotalMilliseconds <= 0 && !msg.ResponseRequired && !_stompConnectionSettings.AlwaysSyncSend &&
                 ( !msg.Persistent || _stompConnectionSettings.AsyncSend || msg.TransactionId != null ) )
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

        public void Start()
        {
            foreach ( MessageConsumer consumer in _consumers.Values )
                consumer.Start();

            Executor?.Start();
        }

        public void Stop()
            => Executor?.Stop();

        protected virtual ProducerInfo CreateProducerInfo( IDestination destination )
            => new ProducerInfo
            {
                ProducerId = GetNextProducerId(),
                Destination = Destination.Transform( destination ),
                DispatchAsync = _stompConnectionSettings.ProducerSettings.DispatchAsync
            };

        internal void Acknowledge()
        {
            lock ( _consumers.SyncRoot )
                foreach ( MessageConsumer consumer in _consumers.Values )
                    consumer.Acknowledge();
        }

        internal void ClearMessagesInProgress()
        {
            Executor?.ClearMessagesInProgress();

            if ( Transacted )
                TransactionContext.ResetTransactionInProgress();

            lock ( _consumers.SyncRoot )
                foreach ( MessageConsumer consumer in _consumers.Values )
                {
                    consumer.InProgressClearRequired();
                    ThreadPool.QueueUserWorkItem( ClearMessages, consumer );
                }
        }

        internal void Redispatch( MessageDispatchChannel channel )
        {
            var messages = channel.EnqueueAll();
            Array.Reverse( messages );

            foreach ( var message in messages )
            {
                Tracer.WarnFormat( "Resending Message Dispatch: ", message.ToString() );
                Executor.ExecuteFirst( message );
            }
        }

        internal void SendAck( MessageAck ack )
            => SendAck( ack, false );

        private void AddConsumer( MessageConsumer consumer )
        {
            if ( _closing )
                return;

            // Registered with Connection before we register at the broker.
            _consumers[consumer.ConsumerId] = consumer;
            Connection.AddDispatcher( consumer.ConsumerId, this );
        }

        private void CheckClosed()
        {
            if ( _closed )
                throw new IllegalStateException( "The Session is Stopped" );
        }

        private static void ClearMessages( Object value )
        {
            var consumer = value as MessageConsumer;
            consumer?.ClearMessagesInProgress();
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

        private ConsumerId GetNextConsumerId()
        {
            var id = new ConsumerId
            {
                ConnectionId = _info.SessionId.ConnectionId,
                SessionId = _info.SessionId.Value,
                Value = Interlocked.Increment( ref _consumerCounter )
            };

            return id;
        }

        private ProducerId GetNextProducerId()
        {
            var id = new ProducerId
            {
                ConnectionId = _info.SessionId.ConnectionId,
                SessionId = _info.SessionId.Value,
                Value = Interlocked.Increment( ref _producerCounter )
            };

            return id;
        }

        private void RemoveConsumer( MessageConsumer consumer )
        {
            Connection.RemoveDispatcher( consumer.ConsumerId );
            if ( !_closing )
                _consumers.Remove( consumer.ConsumerId );
        }

        private void SendAck( MessageAck ack, Boolean lazy )
        {
            if ( lazy || _stompConnectionSettings.SendAcksAsync || IsTransacted )
                Connection.Oneway( ack );
            else
                Connection.SyncRequest( ack );
        }

        ~Session()
        {
            Dispose( false );
        }

        #region Session Transaction Events

        // We delegate the events to the TransactionContext since it knows
        // what the state is at all times.

        public event Action<ISession> TransactionStartedListener
        {
            add { TransactionContext.TransactionStartedListener += value; }
            remove { TransactionContext.TransactionStartedListener += value; }
        }

        public event Action<ISession> TransactionCommittedListener
        {
            add { TransactionContext.TransactionCommittedListener += value; }
            remove { TransactionContext.TransactionCommittedListener += value; }
        }

        public event Action<ISession> TransactionRolledBackListener
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
        public Boolean DispatchAsync => _stompConnectionSettings.DispatchAsync;

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

        public SessionId SessionId => _info.SessionId;

        public TransactionContext TransactionContext { get; }

        /// <summary>
        ///     Gets the request timeout.
        /// </summary>
        /// <value>The request timeout.</value>
        public TimeSpan RequestTimeout => _stompConnectionSettings.RequestTimeout;

        public Boolean Transacted => AcknowledgementMode == AcknowledgementMode.Transactional;

        public AcknowledgementMode AcknowledgementMode { get; }

        public Boolean IsClientAcknowledge => AcknowledgementMode == AcknowledgementMode.ClientAcknowledge;

        public Boolean IsAutoAcknowledge => AcknowledgementMode == AcknowledgementMode.AutoAcknowledge;

        public Boolean IsDupsOkAcknowledge => AcknowledgementMode == AcknowledgementMode.DupsOkAcknowledge;

        public Boolean IsIndividualAcknowledge => AcknowledgementMode == AcknowledgementMode.IndividualAcknowledge;

        public Boolean IsTransacted => AcknowledgementMode == AcknowledgementMode.Transactional;

        public SessionExecutor Executor { get; }

        public Int64 NextDeliveryId => Interlocked.Increment( ref _nextDeliveryId );

        #endregion

        #region ISession Members

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose( Boolean disposing )
        {
            if ( _disposed )
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

            _disposed = true;
        }

        public void Close()
        {
            lock ( _myLock )
            {
                if ( _closed )
                    return;

                try
                {
                    Tracer.InfoFormat( "Closing The Session with Id {0}", _info.SessionId.ToString() );
                    DoClose();
                    Tracer.InfoFormat( "Stopped The Session with Id {0}", _info.SessionId.ToString() );
                }
                catch ( Exception ex )
                {
                    Tracer.ErrorFormat( "Error during session close: {0}", ex );
                }
                finally
                {
                    Connection = null;
                    _closed = true;
                    _closing = false;
                }
            }
        }

        internal void DoClose()
        {
            lock ( _myLock )
            {
                if ( _closed )
                    return;

                try
                {
                    _closing = true;

                    // Stop all message deliveries from this Session
                    Stop();

                    lock ( _consumers.SyncRoot )
                        foreach ( MessageConsumer consumer in _consumers.Values )
                        {
                            consumer.FailureError = Connection.FirstFailureError;
                            consumer.Close();
                        }
                    _consumers.Clear();

                    lock ( _producers.SyncRoot )
                        foreach ( MessageProducer producer in _producers.Values )
                            producer.DoClose();
                    _producers.Clear();

                    // If in a transaction roll it back
                    if ( IsTransacted && TransactionContext.InTransaction )
                        try
                        {
                            TransactionContext.Rollback();
                        }
                        catch
                        {
                            // ignored
                        }

                    Connection.RemoveSession( this );
                }
                catch ( Exception ex )
                {
                    Tracer.ErrorFormat( "Error during session close: {0}", ex );
                }
                finally
                {
                    _closed = true;
                    _closing = false;
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
                _producers[producerId] = producer;
            }
            catch ( Exception )
            {
                producer?.Close();

                throw;
            }

            return producer;
        }

        public IMessageConsumer CreateConsumer( IDestination destination )
            => CreateConsumer( destination, null, false );

        public IMessageConsumer CreateConsumer( IDestination destination, String selector )
            => CreateConsumer( destination, selector, false );

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
                AddConsumer( consumer );

                // lets register the consumer first in case we start dispatching messages immediately
                Connection.SyncRequest( consumer.ConsumerInfo );

                if ( Started )
                    consumer.Start();
            }
            catch ( Exception )
            {
                if ( consumer == null )
                    throw;
                RemoveConsumer( consumer );
                consumer.Close();

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
                AddConsumer( consumer );
                Connection.SyncRequest( consumer.ConsumerInfo );

                if ( Started )
                    consumer.Start();
            }
            catch ( Exception )
            {
                if ( consumer == null )
                    throw;
                RemoveConsumer( consumer );
                consumer.Close();

                throw;
            }

            return consumer;
        }

        public void DeleteDurableConsumer( String name )
        {
            var command = new RemoveSubscriptionInfo
            {
                ConnectionId = Connection.ConnectionId,
                ClientId = Connection.ClientId,
                SubscriptionName = name
            };
            Connection.SyncRequest( command );
        }

        public IQueue GetQueue( String name )
            => new Queue( name );

        public ITopic GetTopic( String name )
            => new Topic( name );

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

        public IBytesMessage CreateBytesMessage()
            => ConfigureMessage( new BytesMessage() ) as IBytesMessage;

        public IBytesMessage CreateBytesMessage( Byte[] body )
        {
            var answer = new BytesMessage { Content = body };
            return ConfigureMessage( answer ) as IBytesMessage;
        }

        public void CommitTransaction()
        {
            if ( !Transacted )
                throw new InvalidOperationException( $"You cannot perform a CommitTransaction() on a non-transacted session. Acknowledgment mode is: {AcknowledgementMode}" );

            TransactionContext.Commit();
        }

        public void RollbackTransaction()
        {
            if ( !Transacted )
                throw new InvalidOperationException( $"You cannot perform a CommitTransaction() on a non-transacted session. Acknowledgment mode is: {AcknowledgementMode}" );

            TransactionContext.Rollback();
        }

        public void Recover()
        {
            CheckClosed();

            if ( AcknowledgementMode == AcknowledgementMode.Transactional )
                throw new IllegalStateException( "Cannot Recover a Transacted Session" );

            lock ( _consumers.SyncRoot )
                foreach ( MessageConsumer consumer in _consumers.Values )
                    consumer.Rollback();
        }

        #endregion
    }
}