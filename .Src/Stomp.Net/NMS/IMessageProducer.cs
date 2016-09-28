#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     An object capable of sending messages to some destination
    /// </summary>
    public interface IMessageProducer : IDisposable
    {
        #region Properties

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