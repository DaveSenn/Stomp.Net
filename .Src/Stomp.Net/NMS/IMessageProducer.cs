#region Usings

using System;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     A delegate that a client can register that will be called each time a Producer's send method is
    ///     called to allow the client to Transform a sent message from one type to another, StreamMessage to
    ///     TextMessage, ObjectMessage to TextMessage containing XML, etc.  This allows a client to create a
    ///     producer that will automatically transform a message to a type that some receiving client is
    ///     capable of processing or adding additional information to a sent message such as additional message
    ///     headers, etc.  For messages that do not need to be processed the client should return null from
    ///     this method, in this case the original message will be sent.
    /// </summary>
    public delegate IMessage ProducerTransformerDelegate( ISession session, IMessageProducer producer, IMessage message );

    /// <summary>
    ///     An object capable of sending messages to some destination
    /// </summary>
    public interface IMessageProducer : IDisposable
    {
        #region Properties

        /// <summary>
        ///     A delegate that is called each time a Message is sent from this Producer which allows
        ///     the application to perform any needed transformations on the Message before it is sent.
        /// </summary>
        ProducerTransformerDelegate ProducerTransformer { get; set; }

        MessageDeliveryMode DeliveryMode { get; set; }

        TimeSpan TimeToLive { get; set; }

        TimeSpan RequestTimeout { get; set; }

        MessagePriority Priority { get; set; }

        Boolean DisableMessageId { get; set; }

        Boolean DisableMessageTimestamp { get; set; }

        #endregion

        /// <summary>
        ///     Close the producer.
        /// </summary>
        void Close();

        /// <summary>
        ///     Sends the message to the default destination for this producer
        /// </summary>
        void Send( IMessage message );

        /// <summary>
        ///     Sends the message to the default destination with the explicit QoS configuration
        /// </summary>
        void Send( IMessage message, MessageDeliveryMode deliveryMode, MessagePriority priority, TimeSpan timeToLive );

        /// <summary>
        ///     Sends the message to the given destination
        /// </summary>
        void Send( IDestination destination, IMessage message );

        /// <summary>
        ///     Sends the message to the given destination with the explicit QoS configuration
        /// </summary>
        void Send( IDestination destination, IMessage message, MessageDeliveryMode deliveryMode, MessagePriority priority, TimeSpan timeToLive );

        #region Factory methods to create messages

        /// <summary>
        ///     Creates a new message with an empty body
        /// </summary>
        IMessage CreateMessage();

        /// <summary>
        ///     Creates a new text message with an empty body
        /// </summary>
        ITextMessage CreateTextMessage();

        /// <summary>
        ///     Creates a new text message with the given body
        /// </summary>
        ITextMessage CreateTextMessage( String text );

        /// <summary>
        ///     Creates a new binary message
        /// </summary>
        IBytesMessage CreateBytesMessage();

        /// <summary>
        ///     Creates a new binary message with the given body
        /// </summary>
        IBytesMessage CreateBytesMessage( Byte[] body );

        #endregion
    }
}