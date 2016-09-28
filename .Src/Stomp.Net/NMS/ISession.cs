#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Represents a single unit of work on an IConnection.
    ///     So the ISession can be used to perform transactional receive and sends
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        ///     Closes the session.  There is no need to close the producers and consumers
        ///     of a closed session.
        /// </summary>
        void Close();

        /// <summary>
        ///     If this is a transactional session then commit all message
        ///     send and acknowledgements for producers and consumers in this session
        /// </summary>
        void CommitTransaction();

        /// <summary>
        ///     Creates a new binary message
        /// </summary>
        IBytesMessage CreateBytesMessage();

        /// <summary>
        ///     Creates a new binary message with the given body
        /// </summary>
        IBytesMessage CreateBytesMessage( Byte[] body );

        /// <summary>
        ///     Creates a consumer of messages on a given destination
        /// </summary>
        IMessageConsumer CreateConsumer( IDestination destination );

        /// <summary>
        ///     Creates a consumer of messages on a given destination with a selector
        /// </summary>
        IMessageConsumer CreateConsumer( IDestination destination, String selector );

        /// <summary>
        ///     Creates a consumer of messages on a given destination with a selector
        /// </summary>
        IMessageConsumer CreateConsumer( IDestination destination, String selector, Boolean noLocal );

        /// <summary>
        ///     Creates a named durable consumer of messages on a given destination with a selector
        /// </summary>
        IMessageConsumer CreateDurableConsumer( ITopic destination, String name, String selector, Boolean noLocal );

        /// <summary>
        ///     Creates a producer of messages
        /// </summary>
        IMessageProducer CreateProducer();

        /// <summary>
        ///     Creates a producer of messages on a given destination
        /// </summary>
        IMessageProducer CreateProducer( IDestination destination );

        /// <summary>
        ///     Creates a temporary queue
        /// </summary>
        ITemporaryQueue CreateTemporaryQueue();

        /// <summary>
        ///     Creates a temporary topic
        /// </summary>
        ITemporaryTopic CreateTemporaryTopic();

        /// <summary>
        ///     Creates a new text message with an empty body
        /// </summary>
        ITextMessage CreateTextMessage();

        /// <summary>
        ///     Creates a new text message with the given body
        /// </summary>
        ITextMessage CreateTextMessage( String text );

        /// <summary>
        ///     Deletes a durable consumer created with CreateDurableConsumer().
        /// </summary>
        /// <param name="name">Name of the durable consumer</param>
        void DeleteDurableConsumer( String name );

        /// <summary>
        ///     Returns the queue for the given name
        /// </summary>
        IQueue GetQueue( String name );

        /// <summary>
        ///     Returns the topic for the given name
        /// </summary>
        ITopic GetTopic( String name );

        /// <summary>
        ///     Stops all Message delivery in this session and restarts it again
        ///     with the oldest not acknowledged message.  Messages that were delivered
        ///     but not acknowledge should have their redelivered property set.
        ///     This is an optional method that may not by implemented by all NMS
        ///     providers, if not implemented an Exception will be thrown.
        ///     Message redelivery is not required to be performed in the original
        ///     order.  It is not valid to call this method on a Transacted Session.
        /// </summary>
        void Recover();

        /// <summary>
        ///     If this is a transactional session then rollback all message
        ///     send and acknowledgements for producers and consumers in this session
        /// </summary>
        void RollbackTransaction();

        event Action<ISession> TransactionCommittedListener;
        event Action<ISession> TransactionRolledBackListener;
        event Action<ISession> TransactionStartedListener;

        #region Attributes

        /// <summary>
        ///     Gets the request timeout.
        /// </summary>
        /// <value>The request timeout.</value>
        TimeSpan RequestTimeout { get; }

        Boolean Transacted { get; }

        AcknowledgementMode AcknowledgementMode { get; }

        #endregion
    }
}