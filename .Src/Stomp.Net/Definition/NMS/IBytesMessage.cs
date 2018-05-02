#region Usings

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     A BytesMessage object is used to send a message containing a stream of uninterpreted
    ///     bytes. It inherits from the Message interface and adds a bytes message body. The
    ///     receiver of the message supplies the interpretation of the bytes.
    ///     This message type is for client encoding of existing message formats. If possible,
    ///     one of the other self-defining message types should be used instead.
    ///     Although the NMS API allows the use of message properties with byte messages, they
    ///     are typically not used, since the inclusion of properties may affect the format.
    ///     When the message is first created, and when ClearBody is called, the body of the
    ///     message is in write-only mode. After the first call to Reset has been made, the
    ///     message body is in read-only mode. After a message has been sent, the client that
    ///     sent it can retain and modify it without affecting the message that has been sent.
    ///     The same message object can be sent multiple times. When a message has been received,
    ///     the provider has called Reset so that the message body is in read-only mode for the
    ///     client.
    ///     If ClearBody is called on a message in read-only mode, the message body is cleared and
    ///     the message is in write-only mode.
    ///     If a client attempts to read a message in write-only mode, a MessageNotReadableException
    ///     is thrown.
    ///     If a client attempts to write a message in read-only mode, a MessageNotWriteableException
    ///     is thrown.
    /// </summary>
    public interface IBytesMessage
    {
        #region Properties

        /// <summary>
        ///     Gets the destination from which the message was received.
        /// </summary>
        /// <value>The destination from which the message was received.</value>
        IDestination FromDestination { get; }

        /// <summary>
        ///     Gets or sets the message content.
        /// </summary>
        /// <value>The message content.</value>
        Byte[] Content { get; set; }

        /// <summary>
        ///     Gets the length of the message content.
        /// </summary>
        /// <value>The length of the message content.</value>

        [PublicAPI]
        Int64 ContentLength { get; }

        /// <summary>
        ///     Gets or sets the message headers.
        /// </summary>
        /// <value>The message headers.</value>
        Dictionary<String, String> Headers { get; }

        /// <summary>
        ///     The correlation ID used to correlate messages from conversations or long running business processes.
        /// </summary>
        String StompCorrelationId { get; set; }

        /// <summary>
        ///     The destination of the message.  This property is set by the IMessageProducer.
        /// </summary>
        IDestination StompDestination { get; set; }

        /// <summary>
        ///     The amount of time for which this message is valid.  Zero if this message does not expire.
        /// </summary>
        TimeSpan StompTimeToLive { get; set; }

        /// <summary>
        ///     The message ID which is set by the provider.
        /// </summary>
        String StompMessageId { get; set; }

        /// <summary>
        ///     Whether or not this message is persistent.
        /// </summary>
        MessageDeliveryMode StompDeliveryMode { get; set; }

        /// <summary>
        ///     The Priority of this message.
        /// </summary>
        MessagePriority StompPriority { get; set; }

        /// <summary>
        ///     Returns true if this message has been redelivered to this or another consumer before being acknowledged
        ///     successfully.
        /// </summary>
        Boolean StompRedelivered { get; set; }

        /// <summary>
        ///     The destination that the consumer of this message should send replies to
        /// </summary>
        IDestination StompReplyTo { get; set; }

        /// <summary>
        ///     The time stamp of when the message was pubished in UTC time.  If the publisher disables setting
        ///     the time stamp on the message, the time will be set to the start of the UNIX epoc (1970-01-01 00:00:00).
        /// </summary>
        DateTime StompTimestamp { get; set; }

        /// <summary>
        ///     The type name of this message.
        /// </summary>
        String StompType { get; set; }

        #endregion

        /// <summary>
        ///     If using client acknowledgement mode on the session, then this method will acknowledge that the
        ///     message has been processed correctly.
        /// </summary>
        void Acknowledge();

        /// <summary>
        ///     Clears out the message body. Clearing a message's body does not clear its header
        ///     values or property entries.
        ///     If this message body was read-only, calling this method leaves the message body in
        ///     the same state as an empty body in a newly created message.
        /// </summary>
        [PublicAPI]
        void ClearBody();
    }
}