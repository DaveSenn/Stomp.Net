#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stomp.Net.Messaging;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Util;
using Stomp.Net.Utilities;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     An object capable of receiving messages from some destination
    /// </summary>
    public class MessageConsumer : Disposable, IMessageConsumer, IDispatcher
    {
        #region Properties

        public Exception FailureError { get; set; }

        #endregion

        #region Ctor

        // Constructor internal to prevent clients from creating an instance.
        internal MessageConsumer( Session session, ConsumerId id, IDestination destination, String name, String selector, Int32 prefetch, Boolean noLocal )
        {
            if ( destination == null )
                throw new InvalidDestinationException( "Consumer cannot receive on null Destinations." );

            _session = session;
            RedeliveryPolicy = _session.Connection.RedeliveryPolicy;

            ConsumerInfo = new ConsumerInfo
            {
                ConsumerId = id,
                Destination = Destination.Transform( destination ),
                SubscriptionName = name,
                Selector = selector,
                PrefetchSize = prefetch,
                MaximumPendingMessageLimit = session.Connection.PrefetchPolicy.MaximumPendingMessageLimit,
                NoLocal = noLocal,
                DispatchAsync = session.DispatchAsync,
                Retroactive = session.Retroactive,
                Exclusive = session.Exclusive,
                Priority = session.Priority,
                AckMode = session.AcknowledgementMode
            };

            // Removed unused consumer. options (ConsumerInfo => "consumer.")
            // TODO: Implement settings?

            // Removed unused message consumer. options (this => "consumer.nms.")
            // TODO: Implement settings?
        }

        #endregion

        public void Dispatch( MessageDispatch dispatch )
        {
            var listener = _listener;

            try
            {
                lock ( _syncRoot )
                {
                    if ( _clearDispatchList )
                    {
                        // we are reconnecting so lets flush the in progress messages
                        _clearDispatchList = false;
                        _unconsumedMessages.Clear();

                        // on resumption a pending delivered ACK will be out of sync with
                        // re-deliveries.
                        _pendingAck = null;
                    }

                    if ( !_unconsumedMessages.Stopped )
                        if ( listener != null && _unconsumedMessages.Started )
                        {
                            var message = CreateStompMessage( dispatch );

                            BeforeMessageIsConsumed( dispatch );

                            try
                            {
                                var expired = !IgnoreExpiration && message.IsExpired();

                                if ( !expired )
                                    listener( message );

                                AfterMessageIsConsumed( dispatch, expired );
                            }
                            catch ( Exception e )
                            {
                                if ( _session.IsAutoAcknowledge || _session.IsIndividualAcknowledge )
                                {
                                    // Redeliver the message
                                }
                                else
                                    // Transacted or Client ACK: Deliver the next message.
                                    AfterMessageIsConsumed( dispatch, false );

                                if ( Tracer.IsErrorEnabled )
                                    Tracer.Error( ConsumerInfo.ConsumerId + " Exception while processing message: " + e );
                            }
                        }
                        else
                            _unconsumedMessages.Enqueue( dispatch );
                }

                if ( ++_dispatchedCount % 1000 != 0 )
                    return;
                _dispatchedCount = 0;
                Thread.Sleep( 1 );
            }
            catch ( Exception e )
            {
                _session.Connection.OnSessionException( _session, e );
            }
        }

        public Boolean Iterate()
        {
            if ( _listener == null )
                return false;

            var dispatch = _unconsumedMessages.DequeueNoWait();
            if ( dispatch == null )
                return false;

            try
            {
                var message = CreateStompMessage( dispatch );
                BeforeMessageIsConsumed( dispatch );

                // ReSharper disable once PossibleNullReferenceException
                _listener( message );
                AfterMessageIsConsumed( dispatch, false );
            }
            catch ( StompException ex )
            {
                _session.Connection.OnSessionException( _session, ex );
            }

            return true;
        }

        public void Start()
        {
            if ( _unconsumedMessages.Stopped )
                return;

            _started.Value = true;
            _unconsumedMessages.Start();
            _session.Executor.Wakeup();
        }

        #region Override of Disposable

        /// <summary>
        ///     Method invoked when the instance gets disposed.
        /// </summary>
        protected override void Disposed()
        {
            try
            {
                Close();
            }
            catch
            {
                // Ignore network errors.
            }
        }

        #endregion

        internal void Acknowledge()
        {
            lock ( _dispatchedMessages )
            {
                // Acknowledge all messages so far.
                var ack = MakeAckForAllDeliveredMessages();

                if ( ack == null )
                    return; // no msgs

                if ( _session.IsTransacted )
                {
                    _session.DoStartTransaction();
                    ack.TransactionId = _session.TransactionContext.TransactionId;
                }

                _session.SendAck( ack );
                _pendingAck = null;

                // Adjust the counters
                _deliveredCounter = Math.Max( 0, _deliveredCounter - _dispatchedMessages.Count );
                _additionalWindowSize = Math.Max( 0, _additionalWindowSize - _dispatchedMessages.Count );

                if ( !_session.IsTransacted )
                    _dispatchedMessages.Clear();
            }
        }

        internal void ClearMessagesInProgress()
        {
            if ( !_inProgressClearRequiredFlag )
                return;

            lock ( _unconsumedMessages )
                if ( _inProgressClearRequiredFlag )
                {
                    _unconsumedMessages.Clear();
                    _synchronizationRegistered = false;

                    // allow dispatch on this connection to resume
                    _inProgressClearRequiredFlag = false;
                }
        }

        internal void InProgressClearRequired()
        {
            _inProgressClearRequiredFlag = true;
            // deal with delivered messages async to avoid lock contention with in progress acks
            _clearDispatchList = true;
        }

        internal void Rollback()
        {
            lock ( _syncRoot )
            lock ( _dispatchedMessages )
            {
                if ( _dispatchedMessages.Count == 0 )
                    return;

                // Only increase the redelivery delay after the first redelivery..
                var lastMd = _dispatchedMessages.First();
                var currentRedeliveryCount = lastMd.Message.RedeliveryCounter;

                _redeliveryDelay = RedeliveryPolicy.RedeliveryDelay( currentRedeliveryCount );

                foreach ( var dispatch in _dispatchedMessages )
                    dispatch.Message.OnMessageRollback();

                if ( RedeliveryPolicy.MaximumRedeliveries >= 0 &&
                     lastMd.Message.RedeliveryCounter > RedeliveryPolicy.MaximumRedeliveries )
                    _redeliveryDelay = 0;
                else
                {
                    // stop the delivery of messages.
                    _unconsumedMessages.Stop();

                    foreach ( var dispatch in _dispatchedMessages )
                        _unconsumedMessages.Enqueue( dispatch, true );

                    if ( _redeliveryDelay > 0 && !_unconsumedMessages.Stopped )
                    {
                        var deadline = DateTime.Now.AddMilliseconds( _redeliveryDelay );
                        ThreadPool.QueueUserWorkItem( RollbackHelper, deadline );
                    }
                    else
                        Start();
                }

                _deliveredCounter -= _dispatchedMessages.Count;
                _dispatchedMessages.Clear();
            }

            // Only redispatch if there's an async _listener otherwise a synchronous
            // consumer will pull them from the local queue.
            if ( _listener != null )
                _session.Redispatch( _unconsumedMessages );
        }

        // ReSharper disable once InconsistentNaming
        private event Action<IBytesMessage> _listener;

        private void AckLater( MessageDispatch dispatch )
        {
            // Don't acknowledge now, but we may need to let the broker know the
            // consumer got the message to expand the pre-fetch window
            if ( _session.IsTransacted )
            {
                _session.DoStartTransaction();

                if ( !_synchronizationRegistered )
                {
                    _synchronizationRegistered = true;
                    _session.TransactionContext.AddSynchronization( new MessageConsumerSynchronization( this ) );
                }
            }

            _deliveredCounter++;

            var oldPendingAck = _pendingAck;

            _pendingAck = new MessageAck
            {
                AckType = (Byte) AckType.ConsumedAck,
                ConsumerId = ConsumerInfo.ConsumerId,
                Destination = dispatch.Destination,
                LastMessageId = dispatch.Message.MessageId,
                MessageCount = _deliveredCounter
            };

            if ( _session.IsTransacted && _session.TransactionContext.InTransaction )
                _pendingAck.TransactionId = _session.TransactionContext.TransactionId;

            if ( oldPendingAck == null )
                _pendingAck.FirstMessageId = _pendingAck.LastMessageId;

            if ( !( 0.5 * ConsumerInfo.PrefetchSize <= _deliveredCounter - _additionalWindowSize ) )
                return;
            _session.SendAck( _pendingAck );
            _pendingAck = null;
            _deliveredCounter = 0;
            _additionalWindowSize = 0;
        }

        private void BeforeMessageIsConsumed( MessageDispatch dispatch )
        {
            lock ( _dispatchedMessages )
                _dispatchedMessages.Insert( 0, dispatch );

            if ( _session.IsTransacted )
                AckLater( dispatch );
        }

        private void Commit()
        {
            lock ( _dispatchedMessages )
                _dispatchedMessages.Clear();

            _redeliveryDelay = 0;
        }

        private BytesMessage CreateStompMessage( MessageDispatch dispatch )
        {
            if ( !( dispatch.Message.Clone() is BytesMessage message ) )
                throw new Exception( $"Message was null => {dispatch.Message}" );

            message.Connection = _session.Connection;

            if ( _session.IsClientAcknowledge )
                message.Acknowledger += DoClientAcknowledge;
            else if ( _session.IsIndividualAcknowledge )
                message.Acknowledger += DoIndividualAcknowledge;
            else
                message.Acknowledger += DoNothingAcknowledge;

            return message;
        }

        private void DoClientAcknowledge( BytesMessage message )
        {
            CheckClosed();
            _session.Acknowledge();
        }

        private void DoIndividualAcknowledge( BytesMessage message )
        {
            MessageDispatch dispatch = null;

            lock ( _dispatchedMessages )
                foreach ( var originalDispatch in _dispatchedMessages.Where( originalDispatch => originalDispatch.Message.MessageId.Equals( message.MessageId ) ) )
                {
                    dispatch = originalDispatch;
                    _dispatchedMessages.Remove( originalDispatch );
                    break;
                }

            if ( dispatch == null )
            {
                if ( Tracer.IsWarnEnabled )
                    Tracer.Warn( $"Attempt to Ack MessageId[{message.MessageId}] failed because the original dispatch is not in the Dispatch List" );
                return;
            }

            var ack = new MessageAck
            {
                AckType = (Byte) AckType.IndividualAck,
                ConsumerId = ConsumerInfo.ConsumerId,
                Destination = dispatch.Destination,
                LastMessageId = dispatch.Message.MessageId,
                MessageCount = 1
            };

            _session.SendAck( ack );
        }

        private static void DoNothingAcknowledge( BytesMessage message )
        {
        }

        private MessageAck MakeAckForAllDeliveredMessages()
        {
            lock ( _dispatchedMessages )
            {
                if ( _dispatchedMessages.Count == 0 )
                    return null;

                var dispatch = _dispatchedMessages.First();
                var ack = new MessageAck
                {
                    AckType = (Byte) AckType.ConsumedAck,
                    ConsumerId = ConsumerInfo.ConsumerId,
                    Destination = dispatch.Destination,
                    LastMessageId = dispatch.Message.MessageId,
                    MessageCount = _dispatchedMessages.Count,
                    FirstMessageId = _dispatchedMessages.First()
                                                        .Message.MessageId
                };

                return ack;
            }
        }

        private void RollbackHelper( Object arg )
        {
            try
            {
                var waitTime = (DateTime) arg - DateTime.Now;

                if ( waitTime.CompareTo( TimeSpan.Zero ) > 0 )
                    Thread.Sleep( (Int32) waitTime.TotalMilliseconds );

                Start();
            }
            catch ( Exception e )
            {
                if ( !_unconsumedMessages.Stopped )
                    _session.Connection.OnSessionException( _session, e );
            }
        }

        #region Fields

        private readonly Atomic<Boolean> _deliveringAcks = new Atomic<Boolean>();
        private readonly List<MessageDispatch> _dispatchedMessages = new List<MessageDispatch>();
        private readonly Atomic<Boolean> _started = new Atomic<Boolean>();
        private readonly Object _syncRoot = new Object();
        private readonly MessageDispatchChannel _unconsumedMessages = new MessageDispatchChannel();
        private Int32 _additionalWindowSize;
        private Boolean _clearDispatchList;
        private Int32 _deliveredCounter;
        private Int32 _dispatchedCount;
        private Boolean _inProgressClearRequiredFlag;
        private MessageAck _pendingAck;
        private Int64 _redeliveryDelay;
        private Session _session;
        private volatile Boolean _synchronizationRegistered;

        #endregion

        #region Property Accessors

        public ConsumerId ConsumerId => ConsumerInfo.ConsumerId;

        public ConsumerInfo ConsumerInfo { get; }

        private Int32 PrefetchSize => ConsumerInfo.PrefetchSize;

        private IRedeliveryPolicy RedeliveryPolicy { get; }

        private Boolean IgnoreExpiration { get; } = false;

        #endregion

        #region IMessageConsumer Members

        public event Action<IBytesMessage> Listener
        {
            add
            {
                CheckClosed();

                if ( PrefetchSize == 0 )
                    throw new StompException( "Cannot set Asynchronous Listener on a Consumer with a zero Prefetch size" );

                var wasStarted = _session.Started;

                if ( wasStarted )
                    _session.Stop();

                _listener += value;
                _session.Redispatch( _unconsumedMessages );

                if ( wasStarted )
                    _session.Start();
            }
            remove => _listener -= value;
        }

        /// <summary>
        ///     If a message is available within the timeout duration it is returned otherwise this method returns null
        /// </summary>
        /// <param name="timeout">An optimal timeout, if not specified infinity will be used.</param>
        /// <returns>Returns the received message, or null in case of a timeout.</returns>
        public IBytesMessage Receive( TimeSpan? timeout = null )
        {
            timeout = timeout ?? TimeSpan.FromMilliseconds( Timeout.Infinite );

            CheckClosed();
            CheckMessageListener();

            var dispatch = Dequeue( timeout.Value );
            if ( dispatch == null )
                return null;

            BeforeMessageIsConsumed( dispatch );
            AfterMessageIsConsumed( dispatch, false );

            return CreateStompMessage( dispatch );
        }

        /// <summary>
        ///     Closes the message consumer.
        /// </summary>
        /// <remarks>
        ///     Clients should close message consumers them when they are not needed.
        ///     This call blocks until a receive or message listener in progress has completed.
        ///     A blocked message consumer receive call returns null when this message consumer is closed.
        /// </remarks>
        public void Close()
        {
            if ( _unconsumedMessages.Stopped )
                return;

            // In case of transaction => close the consumer after the transaction has completed
            if ( _session.IsTransacted && _session.TransactionContext.InTransaction )
                _session.TransactionContext.AddSynchronization( new ConsumerCloseSynchronization( this ) );
            else
                CloseInternal();
        }

        #endregion

        #region Private Members

        /// <summary>
        ///     Used to get an enqueued message from the unconsumedMessages list. The
        ///     amount of time this method blocks is based on the timeout value.  if
        ///     timeout == Timeout.Infinite then it blocks until a message is received.
        ///     if timeout == 0 then it tries to not block at all, it returns a
        ///     message if it is available if timeout > 0 then it blocks up to timeout
        ///     amount of time.  Expired messages will consumed by this method.
        /// </summary>
        private MessageDispatch Dequeue( TimeSpan timeout )
        {
            // Calculate the deadline
            DateTime deadline;
            if ( timeout > TimeSpan.Zero )
                deadline = DateTime.Now + timeout;
            else
                deadline = DateTime.MaxValue;

            while ( true )
            {
                // Check if deadline is reached
                if ( DateTime.Now > deadline )
                    return null;

                // Fetch the message
                var dispatch = _unconsumedMessages.Dequeue( timeout );
                if ( dispatch == null )
                {
                    if ( FailureError != null )
                        throw FailureError.Create();
                    return null;
                }

                if ( dispatch.Message == null )
                    return null;

                if ( IgnoreExpiration || !dispatch.Message.IsExpired() )
                    return dispatch;

                if ( Tracer.IsWarnEnabled )
                    Tracer.Warn( $"{ConsumerInfo.ConsumerId} received expired message: {dispatch.Message.MessageId}" );

                BeforeMessageIsConsumed( dispatch );
                AfterMessageIsConsumed( dispatch, true );
                return null;
            }
        }

        /// <summary>
        ///     Closes the consumer, for real.
        /// </summary>
        private void CloseInternal()
        {
            if ( _unconsumedMessages.Stopped )
                return;

            if ( !_session.IsTransacted )
                lock ( _dispatchedMessages )
                    _dispatchedMessages.Clear();

            _unconsumedMessages.Stop();
            _session.DisposeOf( ConsumerInfo.ConsumerId );

            var removeCommand = new RemoveInfo
            {
                ObjectId = ConsumerInfo.ConsumerId
            };

            _session.Connection.Oneway( removeCommand );
            _session = null;
        }

        /// <summary>
        ///     Checks if the message dispatcher is still open.
        /// </summary>
        private void CheckClosed()
        {
            if ( _unconsumedMessages.Stopped )
                throw new StompException( "The Consumer has been Stopped" );
        }

        /// <summary>
        ///     Checks if the consumer should publish message event or not.
        /// </summary>
        /// <remarks>
        ///     You can not perform a manual receive on a consumer with a event listener.
        /// </remarks>
        private void CheckMessageListener()
        {
            if ( _listener != null )
                throw new StompException( "Cannot perform a Synchronous Receive when there is a registered asynchronous _listener." );
        }

        /// <summary>
        ///     Handles the ACK of the given message.
        /// </summary>
        /// <param name="dispatch">The message.</param>
        /// <param name="expired">A value indicating whether the message was expired or not.</param>
        private void AfterMessageIsConsumed( MessageDispatch dispatch, Boolean expired )
        {
            if ( _unconsumedMessages.Stopped )
                return;

            // Message was expired
            if ( expired )
            {
                lock ( _dispatchedMessages )
                    _dispatchedMessages.Remove( dispatch );
                return;
            }

            // Do noting
            if ( _session.IsClientAcknowledge || _session.IsIndividualAcknowledge || _session.IsTransacted )
                return;

            if ( !_session.IsAutoAcknowledge )
                throw new StompException( "Invalid session state." );

            if ( !_deliveringAcks.CompareAndSet( false, true ) )
                return;

            lock ( _dispatchedMessages )
                if ( _dispatchedMessages.Count > 0 )
                {
                    var ack = new MessageAck
                    {
                        AckType = (Byte) AckType.ConsumedAck,
                        ConsumerId = ConsumerInfo.ConsumerId,
                        Destination = dispatch.Destination,
                        LastMessageId = dispatch.Message.MessageId,
                        MessageCount = 1
                    };

                    _session.SendAck( ack );
                }

            _deliveringAcks.Value = false;
            _dispatchedMessages.Clear();

            throw new StompException( "Invalid session state." );
        }

        #endregion

        #region Nested ISyncronization Types

        private class MessageConsumerSynchronization : ISynchronization
        {
            #region Fields

            private readonly MessageConsumer _consumer;

            #endregion

            #region Ctor

            public MessageConsumerSynchronization( MessageConsumer consumer ) => _consumer = consumer;

            #endregion

            public void AfterCommit()
            {
                _consumer.Commit();
                _consumer._synchronizationRegistered = false;
            }

            public void AfterRollback()
            {
                _consumer.Rollback();
                _consumer._synchronizationRegistered = false;
            }

            public void BeforeEnd()
            {
                _consumer.Acknowledge();
                _consumer._synchronizationRegistered = false;
            }
        }

        private class ConsumerCloseSynchronization : ISynchronization
        {
            #region Fields

            private readonly MessageConsumer _consumer;

            #endregion

            #region Ctor

            public ConsumerCloseSynchronization( MessageConsumer consumer ) => _consumer = consumer;

            #endregion

            public void AfterCommit() =>
                _consumer.CloseInternal();

            public void AfterRollback()
                => _consumer.CloseInternal();

            public void BeforeEnd()
            {
            }
        }

        #endregion
    }
}