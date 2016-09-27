#region Usings

using System;
using System.Collections.Generic;
using Apache.NMS.Stomp.Protocol;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public delegate void AcknowledgeHandler( Message message );

    public class Message : BaseMessage, IMessage
    {
        #region Fields
        
        private TimeSpan _timeToLive = TimeSpan.FromMilliseconds( 0 );

        #endregion

        #region Properties
        
        public IDestination FromDestination
        {
            get { return Destination; }
            set { Destination = Destination.Transform( value ); }
        }

        public Connection Connection { get; set; }

        #endregion

        public void Acknowledge()
        {
            if ( null == Acknowledger )
                throw new NmsException( "No Acknowledger has been associated with this message: " + this );

            Acknowledger( this );
        }

        public virtual void ClearBody()
        {
            ReadOnlyBody = false;
            Content = null;
        }

        public virtual void ClearProperties() => Headers.Clear();

        /// <summary>
        ///     The correlation ID used to correlate messages with conversations or long running business processes
        /// </summary>
        public String NmsCorrelationId
        {
            get { return CorrelationId; }
            set { CorrelationId = value; }
        }

        /// <summary>
        ///     Whether or not this message is persistent
        /// </summary>
        public MessageDeliveryMode NmsDeliveryMode
        {
            get { return Persistent ? MessageDeliveryMode.Persistent : MessageDeliveryMode.NonPersistent; }
            set { Persistent = MessageDeliveryMode.Persistent == value; }
        }

        /// <summary>
        ///     The destination of the message
        /// </summary>
        public IDestination NmsDestination
        {
            get { return Destination; }
            set { Destination = value as Destination; }
        }

        /// <summary>
        ///     The message ID which is set by the provider
        /// </summary>
        public String NmsMessageId
        {
            get { return MessageId?.ToString() ?? String.Empty; }
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
        public MessagePriority NmsPriority
        {
            get { return (MessagePriority) Priority; }
            set { Priority = (Byte) value; }
        }

        /// <summary>
        ///     Returns true if this message has been redelivered to this or another consumer before being acknowledged
        ///     successfully.
        /// </summary>
        public Boolean NmsRedelivered
        {
            get { return RedeliveryCounter > 0; }

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
        public IDestination NmsReplyTo
        {
            get { return ReplyTo; }
            set { ReplyTo = Destination.Transform( value ); }
        }

        /// <summary>
        ///     The time-stamp the broker added to the message.
        /// </summary>
        public DateTime NmsTimestamp
        {
            get { return DateUtils.ToDateTime( Timestamp ); }
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
        public TimeSpan NmsTimeToLive
        {
            get { return _timeToLive; }

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
                {
                    Expiration = 0;
                }
            }
        }

        /// <summary>
        ///     The type name of this message
        /// </summary>
        public String NmsType
        {
            get { return Type; }
            set { Type = value; }
        }

        /// <summary>
        ///     Gets or sets the message headers.
        /// </summary>
        /// <value>The message headers.</value>
        public Dictionary<String, String> Headers { get; } = new Dictionary<String, String>();

        public event AcknowledgeHandler Acknowledger;

        public virtual void BeforeMarshall( StompWireFormat wireFormat )
        {
            /*
             * TODO: Properties
            MarshalledProperties = null;
            if ( _properties != null )
                MarshalledProperties = _properties.Marshal();
            */
        }

        public override Object Clone()
        {
            var cloneMessage = (Message) base.Clone();

            // TODO Properties
            // cloneMessage._propertyHelper = new MessagePropertyIntercepter( cloneMessage, cloneMessage._properties, ReadOnlyProperties );
            // cloneMessage._propertyHelper.AllowByteArrays = false;
            return cloneMessage;
        }

        public override Boolean Equals( Object that )
        {
            if ( that is Message )
                return Equals( (Message) that );
            return false;
        }

        public override Byte GetDataStructureType() => DataStructureTypes.MessageType;

        public override Int32 GetHashCode()
        {
            var id = MessageId;

            return id?.GetHashCode() ?? base.GetHashCode();
        }

        protected void FailIfReadOnlyBody()
        {
            if ( ReadOnlyBody )
                throw new MessageNotWriteableException( "Message is in Read-Only mode." );
        }

        protected void FailIfWriteOnlyBody()
        {
            if ( ReadOnlyBody == false )
                throw new MessageNotReadableException( "Message is in Write-Only mode." );
        }

        private Boolean Equals( Message that )
        {
            var oMsg = that.MessageId;
            var thisMsg = MessageId;

            return thisMsg != null && oMsg != null && oMsg.Equals( thisMsg );
        }

        #region NMS Extension headers

        /// <summary>
        ///     The Message Group ID used to group messages together to the same consumer for the same group ID value
        /// </summary>
        public String NmsxGroupId
        {
            get { return GroupId; }
            set { GroupId = value; }
        }

        /// <summary>
        ///     The Message Group Sequence counter to indicate the position in a group
        /// </summary>
        public Int32 NmsxGroupSeq
        {
            get { return GroupSequence; }
            set { GroupSequence = value; }
        }

        #endregion
    }
}