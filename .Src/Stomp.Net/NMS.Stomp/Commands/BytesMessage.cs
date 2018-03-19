#region Usings

using System;
using System.Collections.Generic;
using Stomp.Net.Util;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     Class representing a byte message.
    /// </summary>
    public class BytesMessage : BaseCommand, IBytesMessage
    {
        #region Fields

        private TimeSpan _timeToLive = TimeSpan.FromMilliseconds( 0 );

        #endregion

        #region Properties

        public IDestination FromDestination
        {
            // ReSharper disable once UnusedMember.Global
            get => Destination;
            set => Destination = Destination.Transform( value );
        }

        public Connection Connection { get; set; }

        /// <summary>
        ///     The Message Group Sequence counter to indicate the position in a group
        /// </summary>
        public Int32 StompGroupSeq
        {
            get => GroupSequence;
            // ReSharper disable once UnusedMember.Global
            set => GroupSequence = value;
        }

        public ProducerId ProducerId { get; set; }

        public Destination Destination { get; set; }

        public TransactionId TransactionId { get; set; }

        public MessageId MessageId { get; set; }

        protected TransactionId OriginalTransactionId { get; set; }

        public String StompGroupId { get; set; }

        protected Int32 GroupSequence { get; set; }

        public String CorrelationId { get; set; }

        public Boolean Persistent { get; set; }

        public Int64 Expiration { get; set; }

        public Byte Priority { get; set; }

        public Destination ReplyTo { get; set; }

        public Int64 Timestamp { get; set; }

        public String Type { get; set; }

        public ConsumerId TargetConsumerId { get; set; }

        public Int32 RedeliveryCounter { get; set; }

        public virtual Boolean ReadOnlyBody { get; set; }

        /// <summery>
        ///     Return an answer of true to the isMessage() query.
        /// </summery>
        public override Boolean IsMessage => true;

        #endregion

        public void Acknowledge()
        {
            if ( null == Acknowledger )
                throw new StompException( "No Acknowledger has been associated with this message: " + this );

            Acknowledger( this );
        }

        public virtual void ClearBody()
        {
            ReadOnlyBody = false;
            Content = null;
        }

        public Byte[] Content { get; set; }

        /// <summary>
        ///     Gets the length of the message content.
        /// </summary>
        /// <value>The length of the message content.</value>
        public Int64 ContentLength => Content.Length;

        /// <summary>
        ///     Gets or sets the message headers.
        /// </summary>
        /// <value>The message headers.</value>
        public Dictionary<String, String> Headers { get; } = new Dictionary<String, String>();

        /// <summary>
        ///     The correlation ID used to correlate messages with conversations or long running business processes
        /// </summary>
        public String StompCorrelationId
        {
            get => CorrelationId;
            set => CorrelationId = value;
        }

        /// <summary>
        ///     Whether or not this message is persistent
        /// </summary>
        public MessageDeliveryMode StompDeliveryMode
        {
            get => Persistent ? MessageDeliveryMode.Persistent : MessageDeliveryMode.NonPersistent;
            set => Persistent = MessageDeliveryMode.Persistent == value;
        }

        /// <summary>
        ///     The destination of the message
        /// </summary>
        public IDestination StompDestination
        {
            get => Destination;
            set => Destination = value as Destination;
        }

        /// <summary>
        ///     The message ID which is set by the provider
        /// </summary>
        public String StompMessageId
        {
            get => MessageId?.ToString() ?? String.Empty;
            set
            {
                if ( value != null )
                    try
                    {
                        var id = new MessageId( value );
                        MessageId = id;
                    }
                    catch ( FormatException )
                    {
                        // we must be some foreign JMS provider or strange user-supplied
                        // String so lets set the IDs to be 1
                        var id = new MessageId();
                        MessageId = id;
                    }
                else
                    MessageId = null;
            }
        }

        /// <summary>
        ///     The Priority on this message
        /// </summary>
        public MessagePriority StompPriority
        {
            get => (MessagePriority) Priority;
            set => Priority = (Byte) value;
        }

        /// <summary>
        ///     Returns true if this message has been redelivered to this or another consumer before being acknowledged
        ///     successfully.
        /// </summary>
        public Boolean StompRedelivered
        {
            get => RedeliveryCounter > 0;

            set
            {
                if ( value )
                {
                    if ( RedeliveryCounter <= 0 )
                        RedeliveryCounter = 1;
                }
                else
                {
                    if ( RedeliveryCounter > 0 )
                        RedeliveryCounter = 0;
                }
            }
        }

        /// <summary>
        ///     The destination that the consumer of this message should send replies to
        /// </summary>
        public IDestination StompReplyTo
        {
            get => ReplyTo;
            set => ReplyTo = Destination.Transform( value );
        }

        /// <summary>
        ///     The time-stamp the broker added to the message.
        /// </summary>
        public DateTime StompTimestamp
        {
            get => DateUtils.ToDateTime( Timestamp );
            set
            {
                Timestamp = DateUtils.ToJavaTimeUtc( value );
                if ( _timeToLive.TotalMilliseconds > 0 )
                    Expiration = Timestamp + (Int64) _timeToLive.TotalMilliseconds;
            }
        }

        /// <summary>
        ///     The time in milliseconds that this message should expire in
        /// </summary>
        public TimeSpan StompTimeToLive
        {
            get => _timeToLive;

            set
            {
                _timeToLive = value;
                if ( _timeToLive.TotalMilliseconds > 0 )
                {
                    var timeStamp = Timestamp;

                    if ( timeStamp == 0 )
                        timeStamp = DateUtils.ToJavaTimeUtc( DateTime.UtcNow );

                    Expiration = timeStamp + (Int64) _timeToLive.TotalMilliseconds;
                }
                else
                    Expiration = 0;
            }
        }

        /// <summary>
        ///     The type name of this message
        /// </summary>
        public String StompType
        {
            get => Type;
            set => Type = value;
        }

        public event Action<BytesMessage> Acknowledger;
        
        /// <summery>
        ///     Clone this object and return a new instance that the caller now owns.
        /// </summery>
        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (BytesMessage) base.Clone();

            if ( MessageId != null )
                o.MessageId = (MessageId) MessageId.Clone();

            return o;
        }

        public override Boolean Equals( Object that )
        {
            if ( that is BytesMessage message )
                return Equals( message );

            return false;
        }

        public override Byte GetDataStructureType()
            => DataStructureTypes.BytesMessageType;

        public override Int32 GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            var id = MessageId;

            return id?.GetHashCode() ?? base.GetHashCode();
        }

        public Boolean IsExpired()
            => Expiration != 0 && DateTime.UtcNow > DateUtils.ToDateTimeUtc( Expiration );

        public void OnMessageRollback()
            => RedeliveryCounter++;

        public void OnSend() => ReadOnlyBody = true;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "ProducerId=" + ProducerId + ", " +
               "Destination=" + Destination + ", " +
               "TransactionId=" + TransactionId + ", " +
               "MessageId=" + MessageId + ", " +
               "OriginalTransactionId=" + OriginalTransactionId + ", " +
               "GroupID=" + StompGroupId + ", " +
               "GroupSequence=" + GroupSequence + ", " +
               "CorrelationId=" + CorrelationId + ", " +
               "Persistent=" + Persistent + ", " +
               "Expiration=" + Expiration + ", " +
               "Priority=" + Priority + ", " +
               "ReplyTo=" + ReplyTo + ", " +
               "Timestamp=" + Timestamp + ", " +
               "Type=" + Type + ", " +
               "Content=" + Content + ", " +
               "TargetConsumerId=" + TargetConsumerId + ", " +
               "RedeliveryCounter=" + RedeliveryCounter +
               "]";

        private Boolean Equals( BytesMessage that )
        {
            var oMsg = that.MessageId;
            var thisMsg = MessageId;

            return thisMsg != null && oMsg != null && oMsg.Equals( thisMsg );
        }
    }
}