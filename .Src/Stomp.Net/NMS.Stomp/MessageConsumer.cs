

#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp
{
    public enum AckType
    {
        ConsumedAck = 1, // Message consumed, discard
        IndividualAck = 2 // Only the given message is to be treated as consumed.
    }

    /// <summary>
    ///     An object capable of receiving messages from some destination
    /// </summary>
    public class MessageConsumer : IMessageConsumer, IDispatcher
    {
        #region Fields

        private readonly Atomic<Boolean> deliveringAcks = new Atomic<Boolean>();
        private readonly LinkedList<MessageDispatch> dispatchedMessages = new LinkedList<MessageDispatch>();
        private readonly MessageTransformation messageTransformation;

        private readonly Atomic<Boolean> started = new Atomic<Boolean>();
        private readonly MessageDispatchChannel unconsumedMessages = new MessageDispatchChannel();
        private Int32 additionalWindowSize;
        private Boolean clearDispatchList;
        private Int32 deliveredCounter;
        private Int32 dispatchedCount;

        protected Boolean disposed;
        private Boolean inProgressClearRequiredFlag;

        private MessageAck pendingAck;
        private Int64 redeliveryDelay;
        private Session session;
        private volatile Boolean synchronizationRegistered;

        #endregion

        #region Properties

        public Exception FailureError { get; set; }

        #endregion

        #region Ctor

        // Constructor internal to prevent clients from creating an instance.
        internal MessageConsumer( Session session, ConsumerId id, Destination destination, String name, String selector, Int32 prefetch, Boolean noLocal )
        {
            if ( destination == null )
                throw new InvalidDestinationException( "Consumer cannot receive on Null Destinations." );

            this.session = session;
            RedeliveryPolicy = this.session.Connection.RedeliveryPolicy;
            messageTransformation = this.session.Connection.MessageTransformation;

            ConsumerInfo = new ConsumerInfo();
            ConsumerInfo.ConsumerId = id;
            ConsumerInfo.Destination = Destination.Transform( destination );
            ConsumerInfo.SubscriptionName = name;
            ConsumerInfo.Selector = selector;
            ConsumerInfo.PrefetchSize = prefetch;
            ConsumerInfo.MaximumPendingMessageLimit = session.Connection.PrefetchPolicy.MaximumPendingMessageLimit;
            ConsumerInfo.NoLocal = noLocal;
            ConsumerInfo.DispatchAsync = session.DispatchAsync;
            ConsumerInfo.Retroactive = session.Retroactive;
            ConsumerInfo.Exclusive = session.Exclusive;
            ConsumerInfo.Priority = session.Priority;
            ConsumerInfo.AckMode = session.AcknowledgementMode;

            // If the destination contained a URI query, then use it to set public properties
            // on the ConsumerInfo
            if ( destination.Options != null )
            {
                // Get options prefixed with "consumer.*"
                var options = URISupport.GetProperties( destination.Options, "consumer." );
                // Extract out custom extension options "consumer.nms.*"
                var customConsumerOptions = URISupport.ExtractProperties( options, "nms." );

                URISupport.SetProperties( ConsumerInfo, options );
                URISupport.SetProperties( this, customConsumerOptions, "nms." );
            }
        }

        #endregion

        public void Dispatch( MessageDispatch dispatch )
        {
            var listener = this.listener;

            try
            {
                lock ( unconsumedMessages.SyncRoot )
                {
                    if ( clearDispatchList )
                    {
                        // we are reconnecting so lets flush the in progress messages
                        clearDispatchList = false;
                        unconsumedMessages.Clear();

                        if ( pendingAck != null )
                        {
                            // on resumption a pending delivered ack will be out of sync with
                            // re-deliveries.
                            if ( Tracer.IsDebugEnabled )
                                Tracer.Debug( "removing pending delivered ack on transport interupt: " + pendingAck );
                            pendingAck = null;
                        }
                    }

                    if ( !unconsumedMessages.Closed )
                        if ( listener != null && unconsumedMessages.Running )
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
                                if ( session.IsAutoAcknowledge || session.IsIndividualAcknowledge )
                                {
                                    // Redeliver the message
                                }
                                else
                                {
                                    // Transacted or Client ack: Deliver the next message.
                                    AfterMessageIsConsumed( dispatch, false );
                                }

                                Tracer.Error( ConsumerInfo.ConsumerId + " Exception while processing message: " + e );
                            }
                        }
                        else
                        {
                            unconsumedMessages.Enqueue( dispatch );
                        }
                }

                if ( ++dispatchedCount % 1000 == 0 )
                {
                    dispatchedCount = 0;
                    Thread.Sleep( 1 );
                }
            }
            catch ( Exception e )
            {
                session.Connection.OnSessionException( session, e );
            }
        }

        public void AfterMessageIsConsumed( MessageDispatch dispatch, Boolean expired )
        {
            if ( unconsumedMessages.Closed )
                return;

            if ( expired )
            {
                lock ( dispatchedMessages )
                    dispatchedMessages.Remove( dispatch );

                // TODO - Not sure if we need to ack this in stomp.
                // AckLater(dispatch, AckType.ConsumedAck);
            }
            else
            {
                if ( session.IsTransacted )
                {
                    // Do nothing.
                }
                else if ( session.IsAutoAcknowledge )
                {
                    if ( deliveringAcks.CompareAndSet( false, true ) )
                    {
                        lock ( dispatchedMessages )
                            if ( dispatchedMessages.Count > 0 )
                            {
                                var ack = new MessageAck();

                                ack.AckType = (Byte) AckType.ConsumedAck;
                                ack.ConsumerId = ConsumerInfo.ConsumerId;
                                ack.Destination = dispatch.Destination;
                                ack.LastMessageId = dispatch.Message.MessageId;
                                ack.MessageCount = 1;

                                session.SendAck( ack );
                            }

                        deliveringAcks.Value = false;
                        dispatchedMessages.Clear();
                    }
                }
                else if ( session.IsClientAcknowledge || session.IsIndividualAcknowledge )
                {
                    // Do nothing.
                }
                else
                {
                    throw new NMSException( "Invalid session state." );
                }
            }
        }

        public void BeforeMessageIsConsumed( MessageDispatch dispatch )
        {
            lock ( dispatchedMessages )
                dispatchedMessages.AddFirst( dispatch );

            if ( session.IsTransacted )
                AckLater( dispatch );
        }

        public void DeliverAcks()
        {
            MessageAck ack = null;

            if ( deliveringAcks.CompareAndSet( false, true ) )
            {
                if ( pendingAck != null && pendingAck.AckType == (Byte) AckType.ConsumedAck )
                {
                    ack = pendingAck;
                    pendingAck = null;
                }

                if ( pendingAck != null )
                {
                    var ackToSend = ack;

                    try
                    {
                        session.SendAck( ackToSend );
                    }
                    catch ( Exception e )
                    {
                        Tracer.DebugFormat( "{0} : Failed to send ack, {1}", ConsumerInfo.ConsumerId, e );
                    }
                }
                else
                {
                    deliveringAcks.Value = false;
                }
            }
        }

        public Boolean Iterate()
        {
            if ( listener != null )
            {
                var dispatch = unconsumedMessages.DequeueNoWait();
                if ( dispatch != null )
                {
                    try
                    {
                        var message = CreateStompMessage( dispatch );
                        BeforeMessageIsConsumed( dispatch );
                        listener( message );
                        AfterMessageIsConsumed( dispatch, false );
                    }
                    catch ( NMSException e )
                    {
                        session.Connection.OnSessionException( session, e );
                    }

                    return true;
                }
            }

            return false;
        }

        public void Start()
        {
            if ( unconsumedMessages.Closed )
                return;

            started.Value = true;
            unconsumedMessages.Start();
            session.Executor.Wakeup();
        }

        public void Stop()
        {
            started.Value = false;
            unconsumedMessages.Stop();
        }

        protected void DoClientAcknowledge( Message message )
        {
            CheckClosed();
            Tracer.Debug( "Sending Client Ack:" );
            session.Acknowledge();
        }

        protected void DoIndividualAcknowledge( Message message )
        {
            MessageDispatch dispatch = null;

            lock ( dispatchedMessages )
                foreach ( var originalDispatch in dispatchedMessages )
                    if ( originalDispatch.Message.MessageId.Equals( message.MessageId ) )
                    {
                        dispatch = originalDispatch;
                        dispatchedMessages.Remove( originalDispatch );
                        break;
                    }

            if ( dispatch == null )
            {
                Tracer.DebugFormat( "Attempt to Ack MessageId[{0}] failed because the original dispatch is not in the Dispatch List", message.MessageId );
                return;
            }

            var ack = new MessageAck();

            ack.AckType = (Byte) AckType.IndividualAck;
            ack.ConsumerId = ConsumerInfo.ConsumerId;
            ack.Destination = dispatch.Destination;
            ack.LastMessageId = dispatch.Message.MessageId;
            ack.MessageCount = 1;

            Tracer.Debug( "Sending Individual Ack for MessageId: " + ack.LastMessageId );
            session.SendAck( ack );
        }

        protected void DoNothingAcknowledge( Message message )
        {
        }

        internal void Acknowledge()
        {
            lock ( dispatchedMessages )
            {
                // Acknowledge all messages so far.
                var ack = MakeAckForAllDeliveredMessages();

                if ( ack == null )
                    return; // no msgs

                if ( session.IsTransacted )
                {
                    session.DoStartTransaction();
                    ack.TransactionId = session.TransactionContext.TransactionId;
                }

                session.SendAck( ack );
                pendingAck = null;

                // Adjust the counters
                deliveredCounter = Math.Max( 0, deliveredCounter - dispatchedMessages.Count );
                additionalWindowSize = Math.Max( 0, additionalWindowSize - dispatchedMessages.Count );

                if ( !session.IsTransacted )
                    dispatchedMessages.Clear();
            }
        }

        internal void ClearMessagesInProgress()
        {
            if ( inProgressClearRequiredFlag )
                lock ( unconsumedMessages )
                    if ( inProgressClearRequiredFlag )
                    {
                        if ( Tracer.IsDebugEnabled )
                            Tracer.Debug( ConsumerId + " clearing dispatched list (" +
                                          unconsumedMessages.Count + ") on transport interrupt" );

                        unconsumedMessages.Clear();
                        synchronizationRegistered = false;

                        // allow dispatch on this connection to resume
                        session.Connection.TransportInterruptionProcessingComplete();
                        inProgressClearRequiredFlag = false;
                    }
        }

        internal void Commit()
        {
            lock ( dispatchedMessages )
                dispatchedMessages.Clear();

            redeliveryDelay = 0;
        }

        internal void InProgressClearRequired()
        {
            inProgressClearRequiredFlag = true;
            // deal with delivered messages async to avoid lock contention with in progress acks
            clearDispatchList = true;
        }

        internal void Rollback()
        {
            lock ( unconsumedMessages.SyncRoot )
                lock ( dispatchedMessages )
                {
                    Tracer.DebugFormat( "Rollback started, rolling back {0} message",
                                        dispatchedMessages.Count );

                    if ( dispatchedMessages.Count == 0 )
                        return;

                    // Only increase the redelivery delay after the first redelivery..
                    var lastMd = dispatchedMessages.First.Value;
                    var currentRedeliveryCount = lastMd.Message.RedeliveryCounter;

                    redeliveryDelay = RedeliveryPolicy.RedeliveryDelay( currentRedeliveryCount );

                    foreach ( var dispatch in dispatchedMessages )
                        dispatch.Message.OnMessageRollback();

                    if ( RedeliveryPolicy.MaximumRedeliveries >= 0 &&
                         lastMd.Message.RedeliveryCounter > RedeliveryPolicy.MaximumRedeliveries )
                    {
                        redeliveryDelay = 0;
                    }
                    else
                    {
                        // stop the delivery of messages.
                        unconsumedMessages.Stop();

                        foreach ( var dispatch in dispatchedMessages )
                            unconsumedMessages.EnqueueFirst( dispatch );

                        if ( redeliveryDelay > 0 && !unconsumedMessages.Closed )
                        {
                            Tracer.DebugFormat( "Rollback delayed for {0} seconds", redeliveryDelay );
                            var deadline = DateTime.Now.AddMilliseconds( redeliveryDelay );
                            ThreadPool.QueueUserWorkItem( RollbackHelper, deadline );
                        }
                        else
                        {
                            Start();
                        }
                    }

                    deliveredCounter -= dispatchedMessages.Count;
                    dispatchedMessages.Clear();
                }

            // Only redispatch if there's an async listener otherwise a synchronous
            // consumer will pull them from the local queue.
            if ( listener != null )
                session.Redispatch( unconsumedMessages );
        }

        private void AckLater( MessageDispatch dispatch )
        {
            // Don't acknowledge now, but we may need to let the broker know the
            // consumer got the message to expand the pre-fetch window
            if ( session.IsTransacted )
            {
                session.DoStartTransaction();

                if ( !synchronizationRegistered )
                {
                    synchronizationRegistered = true;
                    session.TransactionContext.AddSynchronization( new MessageConsumerSynchronization( this ) );
                }
            }

            deliveredCounter++;

            var oldPendingAck = pendingAck;

            pendingAck = new MessageAck();
            pendingAck.AckType = (Byte) AckType.ConsumedAck;
            pendingAck.ConsumerId = ConsumerInfo.ConsumerId;
            pendingAck.Destination = dispatch.Destination;
            pendingAck.LastMessageId = dispatch.Message.MessageId;
            pendingAck.MessageCount = deliveredCounter;

            if ( session.IsTransacted && session.TransactionContext.InTransaction )
                pendingAck.TransactionId = session.TransactionContext.TransactionId;

            if ( oldPendingAck == null )
                pendingAck.FirstMessageId = pendingAck.LastMessageId;

            if ( 0.5 * ConsumerInfo.PrefetchSize <= deliveredCounter - additionalWindowSize )
            {
                session.SendAck( pendingAck );
                pendingAck = null;
                deliveredCounter = 0;
                additionalWindowSize = 0;
            }
        }

        private void CheckClosed()
        {
            if ( unconsumedMessages.Closed )
                throw new NMSException( "The Consumer has been Closed" );
        }

        private void CheckMessageListener()
        {
            if ( listener != null )
                throw new NMSException( "Cannot perform a Synchronous Receive when there is a registered asynchronous listener." );
        }

        private Message CreateStompMessage( MessageDispatch dispatch )
        {
            var message = dispatch.Message.Clone() as Message;

            if ( ConsumerTransformer != null )
            {
                var transformed = ConsumerTransformer( session, this, message );
                if ( transformed != null )
                    message = messageTransformation.TransformMessage<Message>( transformed );
            }

            message.Connection = session.Connection;

            if ( session.IsClientAcknowledge )
                message.Acknowledger += DoClientAcknowledge;
            else if ( session.IsIndividualAcknowledge )
                message.Acknowledger += DoIndividualAcknowledge;
            else
                message.Acknowledger += DoNothingAcknowledge;

            return message;
        }

        /// <summary>
        ///     Used to get an enqueued message from the unconsumedMessages list. The
        ///     amount of time this method blocks is based on the timeout value.  if
        ///     timeout == Timeout.Infinite then it blocks until a message is received.
        ///     if timeout == 0 then it it tries to not block at all, it returns a
        ///     message if it is available if timeout > 0 then it blocks up to timeout
        ///     amount of time.  Expired messages will consumed by this method.
        /// </summary>
        private MessageDispatch Dequeue( TimeSpan timeout )
        {
            var deadline = DateTime.Now;

            if ( timeout > TimeSpan.Zero )
                deadline += timeout;

            while ( true )
            {
                var dispatch = unconsumedMessages.Dequeue( timeout );

                // Grab a single date/time for calculations to avoid timing errors.
                var dispatchTime = DateTime.Now;

                if ( dispatch == null )
                {
                    if ( timeout > TimeSpan.Zero && !unconsumedMessages.Closed )
                    {
                        if ( dispatchTime > deadline )
                            timeout = TimeSpan.Zero;
                        else
                            timeout = deadline - dispatchTime;
                    }
                    else
                    {
                        if ( FailureError != null )
                            throw NMSExceptionSupport.Create( FailureError );
                        return null;
                    }
                }
                else if ( dispatch.Message == null )
                {
                    return null;
                }
                else if ( !IgnoreExpiration && dispatch.Message.IsExpired() )
                {
                    Tracer.DebugFormat( "{0} received expired message: {1}", ConsumerInfo.ConsumerId, dispatch.Message.MessageId );

                    BeforeMessageIsConsumed( dispatch );
                    AfterMessageIsConsumed( dispatch, true );
                    // Refresh the dispatch time
                    dispatchTime = DateTime.Now;

                    if ( timeout > TimeSpan.Zero && !unconsumedMessages.Closed )
                        if ( dispatchTime > deadline )
                            timeout = TimeSpan.Zero;
                        else
                            timeout = deadline - dispatchTime;
                }
                else
                {
                    return dispatch;
                }
            }
        }

        private event MessageListener listener;

        private MessageAck MakeAckForAllDeliveredMessages()
        {
            lock ( dispatchedMessages )
            {
                if ( dispatchedMessages.Count == 0 )
                    return null;

                var dispatch = dispatchedMessages.First.Value;
                var ack = new MessageAck();

                ack.AckType = (Byte) AckType.ConsumedAck;
                ack.ConsumerId = ConsumerInfo.ConsumerId;
                ack.Destination = dispatch.Destination;
                ack.LastMessageId = dispatch.Message.MessageId;
                ack.MessageCount = dispatchedMessages.Count;
                ack.FirstMessageId = dispatchedMessages.Last.Value.Message.MessageId;

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
                if ( !unconsumedMessages.Closed )
                    session.Connection.OnSessionException( session, e );
            }
        }

        ~MessageConsumer()
        {
            Dispose( false );
        }

        #region Property Accessors

        public ConsumerId ConsumerId
        {
            get { return ConsumerInfo.ConsumerId; }
        }

        public ConsumerInfo ConsumerInfo { get; }

        public Int32 PrefetchSize
        {
            get { return ConsumerInfo.PrefetchSize; }
        }

        public IRedeliveryPolicy RedeliveryPolicy { get; set; }

        // Custom Options

        public Boolean IgnoreExpiration { get; set; } = false;

        #endregion

        #region IMessageConsumer Members

        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        public event MessageListener Listener
        {
            add
            {
                CheckClosed();

                if ( PrefetchSize == 0 )
                    throw new NMSException( "Cannot set Asynchronous Listener on a Consumer with a zero Prefetch size" );

                var wasStarted = session.Started;

                if ( wasStarted )
                    session.Stop();

                listener += value;
                session.Redispatch( unconsumedMessages );

                if ( wasStarted )
                    session.Start();
            }
            remove { listener -= value; }
        }

        public IMessage Receive()
        {
            CheckClosed();
            CheckMessageListener();

            var dispatch = Dequeue( TimeSpan.FromMilliseconds( -1 ) );

            if ( dispatch == null )
                return null;

            BeforeMessageIsConsumed( dispatch );
            AfterMessageIsConsumed( dispatch, false );

            return CreateStompMessage( dispatch );
        }

        public IMessage Receive( TimeSpan timeout )
        {
            CheckClosed();
            CheckMessageListener();

            var dispatch = Dequeue( timeout );

            if ( dispatch == null )
                return null;

            BeforeMessageIsConsumed( dispatch );
            AfterMessageIsConsumed( dispatch, false );

            return CreateStompMessage( dispatch );
        }

        public IMessage ReceiveNoWait()
        {
            CheckClosed();
            CheckMessageListener();

            var dispatch = Dequeue( TimeSpan.Zero );

            if ( dispatch == null )
                return null;

            BeforeMessageIsConsumed( dispatch );
            AfterMessageIsConsumed( dispatch, false );

            return CreateStompMessage( dispatch );
        }

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
            if ( !unconsumedMessages.Closed )
                if ( session.IsTransacted && session.TransactionContext.InTransaction )
                    session.TransactionContext.AddSynchronization( new ConsumerCloseSynchronization( this ) );
                else
                    DoClose();
        }

        internal void DoClose()
        {
            if ( !unconsumedMessages.Closed )
            {
                Tracer.Debug( "Closing down the Consumer" );

                if ( !session.IsTransacted )
                    lock ( dispatchedMessages )
                        dispatchedMessages.Clear();

                unconsumedMessages.Close();
                session.DisposeOf( ConsumerInfo.ConsumerId );

                var removeCommand = new RemoveInfo();
                removeCommand.ObjectId = ConsumerInfo.ConsumerId;

                session.Connection.Oneway( removeCommand );
                session = null;

                Tracer.Debug( "Consumer instnace Closed." );
            }
        }

        #endregion

        #region Nested ISyncronization Types

        class MessageConsumerSynchronization : ISynchronization
        {
            #region Fields

            private readonly MessageConsumer consumer;

            #endregion

            #region Ctor

            public MessageConsumerSynchronization( MessageConsumer consumer )
            {
                this.consumer = consumer;
            }

            #endregion

            public void AfterCommit()
            {
                consumer.Commit();
                consumer.synchronizationRegistered = false;
            }

            public void AfterRollback()
            {
                consumer.Rollback();
                consumer.synchronizationRegistered = false;
            }

            public void BeforeEnd()
            {
                consumer.Acknowledge();
                consumer.synchronizationRegistered = false;
            }
        }

        class ConsumerCloseSynchronization : ISynchronization
        {
            #region Fields

            private readonly MessageConsumer consumer;

            #endregion

            #region Ctor

            public ConsumerCloseSynchronization( MessageConsumer consumer )
            {
                this.consumer = consumer;
            }

            #endregion

            public void AfterCommit()
            {
                consumer.DoClose();
            }

            public void AfterRollback()
            {
                consumer.DoClose();
            }

            public void BeforeEnd()
            {
            }
        }

        #endregion
    }
}