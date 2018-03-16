#region Usings

using System;
using System.Collections.Generic;
using Stomp.Net.Stomp.Protocol;
using Stomp.Net.Util;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public abstract class Message : BaseMessage, IMessage
    {
        #region Fields

        private TimeSpan _timeToLive = TimeSpan.FromMilliseconds( 0 );

        #endregion

        #region Properties

        public IDestination FromDestination
        {
            get => Destination;
            set => Destination = Destination.Transform( value );
        }

        public Connection Connection { get; set; }

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

        public event Action<Message> Acknowledger;

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
            => (Message) base.Clone();

        public override Boolean Equals( Object that )
        {
            if ( that is Message message )
                return Equals( message );

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

        private Boolean Equals( BaseMessage that )
        {
            var oMsg = that.MessageId;
            var thisMsg = MessageId;

            return thisMsg != null && oMsg != null && oMsg.Equals( thisMsg );
        }

        #region NMS Extension headers

        /*
        /// <summary>
        ///     The Message Group ID used to group messages together to the same consumer for the same group ID value
        /// </summary>
        public String StompGroupId
        {
            get => GroupId;
            set => GroupId = value;
        }*/

        /// <summary>
        ///     The Message Group Sequence counter to indicate the position in a group
        /// </summary>
        public Int32 StompGroupSeq
        {
            get => GroupSequence;
            set => GroupSequence = value;
        }

        #endregion
    }
}