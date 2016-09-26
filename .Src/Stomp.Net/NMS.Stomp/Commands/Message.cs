#region Usings

using System;
using Apache.NMS.Stomp.Protocol;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public delegate void AcknowledgeHandler( Message message );

    public class Message : BaseMessage, IMessage, MarshallAware
    {
        #region Fields

        private PrimitiveMap properties;
        private MessagePropertyIntercepter propertyHelper;

        private TimeSpan timeToLive = TimeSpan.FromMilliseconds( 0 );

        #endregion

        #region Properties

        public override Boolean ReadOnlyProperties
        {
            get { return base.ReadOnlyProperties; }

            set
            {
                if ( propertyHelper != null )
                    propertyHelper.ReadOnly = value;
                base.ReadOnlyProperties = value;
            }
        }

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
                throw new NMSException( "No Acknowledger has been associated with this message: " + this );

            Acknowledger( this );
        }

        public virtual void ClearBody()
        {
            ReadOnlyBody = false;
            Content = null;
        }

        public virtual void ClearProperties()
        {
            MarshalledProperties = null;
            ReadOnlyProperties = false;
            Properties.Clear();
        }

        /// <summary>
        ///     The correlation ID used to correlate messages with conversations or long running business processes
        /// </summary>
        public String NMSCorrelationID
        {
            get { return CorrelationId; }
            set { CorrelationId = value; }
        }

        /// <summary>
        ///     Whether or not this message is persistent
        /// </summary>
        public MsgDeliveryMode NMSDeliveryMode
        {
            get { return Persistent ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent; }
            set { Persistent = MsgDeliveryMode.Persistent == value; }
        }

        /// <summary>
        ///     The destination of the message
        /// </summary>
        public IDestination NMSDestination
        {
            get { return Destination; }
            set { Destination = value as Destination; }
        }

        /// <summary>
        ///     The message ID which is set by the provider
        /// </summary>
        public String NMSMessageId
        {
            get
            {
                if ( null != MessageId )
                    return MessageId.ToString();

                return String.Empty;
            }

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
        public MsgPriority NMSPriority
        {
            get { return (MsgPriority) Priority; }
            set { Priority = (Byte) value; }
        }

        /// <summary>
        ///     Returns true if this message has been redelivered to this or another consumer before being acknowledged
        ///     successfully.
        /// </summary>
        public Boolean NMSRedelivered
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
        public IDestination NMSReplyTo
        {
            get { return ReplyTo; }
            set { ReplyTo = Destination.Transform( value ); }
        }

        /// <summary>
        ///     The timestamp the broker added to the message
        /// </summary>
        public DateTime NMSTimestamp
        {
            get { return DateUtils.ToDateTime( Timestamp ); }
            set
            {
                Timestamp = DateUtils.ToJavaTimeUtc( value );
                if ( timeToLive.TotalMilliseconds > 0 )
                    Expiration = Timestamp + (Int64) timeToLive.TotalMilliseconds;
            }
        }

        /// <summary>
        ///     The time in milliseconds that this message should expire in
        /// </summary>
        public TimeSpan NMSTimeToLive
        {
            get { return timeToLive; }

            set
            {
                timeToLive = value;
                if ( timeToLive.TotalMilliseconds > 0 )
                {
                    var timeStamp = Timestamp;

                    if ( timeStamp == 0 )
                        timeStamp = DateUtils.ToJavaTimeUtc( DateTime.UtcNow );

                    Expiration = timeStamp + (Int64) timeToLive.TotalMilliseconds;
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
        public String NMSType
        {
            get { return Type; }
            set { Type = value; }
        }

        public IPrimitiveMap Properties
        {
            get
            {
                if ( null == properties )
                {
                    properties = PrimitiveMap.Unmarshal( MarshalledProperties );
                    propertyHelper = new MessagePropertyIntercepter( this, properties, ReadOnlyProperties );
                    propertyHelper.AllowByteArrays = false;
                }

                return propertyHelper;
            }
        }

        public override void BeforeMarshall( StompWireFormat wireFormat )
        {
            MarshalledProperties = null;
            if ( properties != null )
                MarshalledProperties = properties.Marshal();
        }

        public event AcknowledgeHandler Acknowledger;

        public override Object Clone()
        {
            var cloneMessage = (Message) base.Clone();

            cloneMessage.propertyHelper = new MessagePropertyIntercepter( cloneMessage, cloneMessage.properties, ReadOnlyProperties );
            cloneMessage.propertyHelper.AllowByteArrays = false;
            return cloneMessage;
        }

        public override Boolean Equals( Object that )
        {
            if ( that is Message )
                return Equals( (Message) that );
            return false;
        }

        public virtual Boolean Equals( Message that )
        {
            var oMsg = that.MessageId;
            var thisMsg = MessageId;

            return thisMsg != null && oMsg != null && oMsg.Equals( thisMsg );
        }

        public override Byte GetDataStructureType() => DataStructureTypes.MessageType;

        public override Int32 GetHashCode()
        {
            var id = MessageId;

            return id != null ? id.GetHashCode() : base.GetHashCode();
        }

        public Object GetObjectProperty( String name ) => Properties[name];

        // MarshallAware interface
        public override Boolean IsMarshallAware() => true;

        public void SetObjectProperty( String name, Object value )
        {
            Properties[name] = value;
        }

        public static Message Transform( IMessage message ) => (Message) message;

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

        #region NMS Extension headers

        /// <summary>
        ///     Returns the number of times this message has been redelivered to other consumers without being acknowledged
        ///     successfully.
        /// </summary>
        public Int32 NMSXDeliveryCount
        {
            get { return RedeliveryCounter + 1; }
        }

        /// <summary>
        ///     The Message Group ID used to group messages together to the same consumer for the same group ID value
        /// </summary>
        public String NMSXGroupID
        {
            get { return GroupID; }
            set { GroupID = value; }
        }

        /// <summary>
        ///     The Message Group Sequence counter to indicate the position in a group
        /// </summary>
        public Int32 NMSXGroupSeq
        {
            get { return GroupSequence; }
            set { GroupSequence = value; }
        }

        /// <summary>
        ///     Returns the ID of the producers transaction
        /// </summary>
        public String NMSXProducerTXID
        {
            get
            {
                var txnId = OriginalTransactionId;
                if ( null == txnId )
                    txnId = TransactionId;

                if ( null != txnId )
                    return "" + txnId.Value;

                return String.Empty;
            }
        }

        #endregion
    }
}