#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net;

/// <summary>
///     An object capable of sending messages to some destination
/// </summary>
public interface IMessageProducer : IDisposable
{
    /// <summary>
    ///     Close the producer.
    /// </summary>
    [PublicAPI]
    void Close();

    /// <summary>
    ///     Sends the message to the default destination for this producer
    /// </summary>
    [PublicAPI]
    void Send( IBytesMessage message );

    /// <summary>
    ///     Sends the message to the default destination with the explicit QoS configuration
    /// </summary>
    [PublicAPI]
    void Send( IBytesMessage message, MessageDeliveryMode deliveryMode, MessagePriority priority, TimeSpan timeToLive );

    /// <summary>
    ///     Sends the message to the given destination
    /// </summary>
    [PublicAPI]
    void Send( IDestination destination, IBytesMessage message );

    /// <summary>
    ///     Sends the message to the given destination with the explicit QoS configuration
    /// </summary>
    [PublicAPI]
    void Send( IDestination destination, IBytesMessage message, MessageDeliveryMode deliveryMode, MessagePriority priority, TimeSpan timeToLive );

    #region Properties

    [PublicAPI]
    MessageDeliveryMode DeliveryMode { get; set; }

    [PublicAPI]
    TimeSpan TimeToLive { get; set; }

    [PublicAPI]
    TimeSpan RequestTimeout { get; set; }

    [PublicAPI]
    MessagePriority Priority { get; set; }

    [PublicAPI]
    Boolean DisableMessageId { get; set; }

    [PublicAPI]
    Boolean DisableMessageTimestamp { get; set; }

    #endregion

    #region Factory methods to create messages

    /// <summary>
    ///     Creates a new binary message
    /// </summary>
    [PublicAPI]
    IBytesMessage CreateBytesMessage();

    /// <summary>
    ///     Creates a new binary message with the given body
    /// </summary>
    [PublicAPI]
    IBytesMessage CreateBytesMessage( Byte[] body );

    #endregion
}