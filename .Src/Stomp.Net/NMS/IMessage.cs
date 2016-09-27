#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Represents a message either to be sent to a message broker or received from a message broker.
    /// </summary>
    public interface IMessage
    {
        #region Properties

        /// <summary>
        ///     Provides access to the message properties (headers).
        /// </summary>
        IPrimitiveMap Properties { get; }
        //Dictionary<String, String> Properties { get; }

        /// <summary>
        ///     The correlation ID used to correlate messages from conversations or long running business processes.
        /// </summary>
        String NMSCorrelationID { get; set; }

        /// <summary>
        ///     The destination of the message.  This property is set by the IMessageProducer.
        /// </summary>
        IDestination NMSDestination { get; set; }

        /// <summary>
        ///     The amount of time for which this message is valid.  Zero if this message does not expire.
        /// </summary>
        TimeSpan NMSTimeToLive { get; set; }

        /// <summary>
        ///     The message ID which is set by the provider.
        /// </summary>
        String NMSMessageId { get; set; }

        /// <summary>
        ///     Whether or not this message is persistent.
        /// </summary>
        MessageDeliveryMode NMSDeliveryMode { get; set; }

        /// <summary>
        ///     The Priority of this message.
        /// </summary>
        MessagePriority NMSPriority { get; set; }

        /// <summary>
        ///     Returns true if this message has been redelivered to this or another consumer before being acknowledged
        ///     successfully.
        /// </summary>
        Boolean NMSRedelivered { get; set; }

        /// <summary>
        ///     The destination that the consumer of this message should send replies to
        /// </summary>
        IDestination NMSReplyTo { get; set; }

        /// <summary>
        ///     The timestamp of when the message was pubished in UTC time.  If the publisher disables setting
        ///     the timestamp on the message, the time will be set to the start of the UNIX epoc (1970-01-01 00:00:00).
        /// </summary>
        DateTime NMSTimestamp { get; set; }

        /// <summary>
        ///     The type name of this message.
        /// </summary>
        String NMSType { get; set; }

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
        void ClearBody();

        /// <summary>
        ///     Clears a message's properties.
        ///     The message's header fields and body are not cleared.
        /// </summary>
        void ClearProperties();
    }
}