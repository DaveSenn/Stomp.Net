#region Usings

using System;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     An object capable of sending messages to some destination
    /// </summary>
    public class MessageProducer : IMessageProducer
    {
        #region Fields

        private readonly Object _closedLock = new Object();
        private readonly ProducerInfo _info;

        private readonly MessageTransformation _messageTransformation;
        private Boolean _closed;
        private Boolean _disposed;

        private Int32 _producerSequenceId;

        private Session _session;

        #endregion

        #region Properties

        public ProducerId ProducerId => _info.ProducerId;

        #endregion

        #region Ctor

        public MessageProducer( Session session, ProducerInfo info )
        {
            _session = session;
            _info = info;
            RequestTimeout = session.RequestTimeout;
            _messageTransformation = session.Connection.MessageTransformation;
        }

        #endregion

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void Close()
        {
            lock ( _closedLock )
            {
                if ( _closed )
                    return;

                DoClose();
                _session = null;
            }
        }

        public MessageDeliveryMode DeliveryMode { get; set; } = NmsConstants.DefaultDeliveryMode;

        public Boolean DisableMessageId { get; set; } = false;

        public Boolean DisableMessageTimestamp { get; set; } = false;

        public MessagePriority Priority { get; set; } = NmsConstants.DefaultPriority;
        
        public TimeSpan RequestTimeout { get; set; }

        public void Send( IMessage message ) => Send( _info.Destination, message, DeliveryMode, Priority, TimeToLive );

        public void Send( IDestination destination, IMessage message ) => Send( destination, message, DeliveryMode, Priority, TimeToLive );

        public void Send( IMessage message, MessageDeliveryMode deliveryMode, MessagePriority priority, TimeSpan timeToLive )
            => Send( _info.Destination, message, deliveryMode, priority, timeToLive );

        public void Send( IDestination destination, IMessage message, MessageDeliveryMode deliveryMode, MessagePriority priority, TimeSpan timeToLive )
        {
            if ( null == destination )
            {
                // See if this producer was created without a destination.
                if ( null == _info.Destination )
                    throw new NotSupportedException();

                throw new InvalidDestinationException(
                    $"The producer was created with a destination, but an invalid destination was specified. => Destination: '{_info.Destination}'" );
            }

            Destination dest;

            if ( Equals( destination, _info.Destination ) )
                dest = destination as Destination;
            else if ( _info.Destination == null )
                dest = Destination.Transform( destination );
            else
                throw new NotSupportedException( "This producer can only send messages to: " + _info.Destination.PhysicalName );
            
            var stompMessage = _messageTransformation.TransformMessage<Message>( message );

            stompMessage.ProducerId = _info.ProducerId;
            stompMessage.FromDestination = dest;
            stompMessage.NmsDeliveryMode = deliveryMode;
            stompMessage.NmsPriority = priority;

            // Always set the message Id regardless of the disable flag.
            var id = new MessageId
            {
                ProducerId = _info.ProducerId,
                ProducerSequenceId = Interlocked.Increment( ref _producerSequenceId )
            };
            stompMessage.MessageId = id;

            if ( !DisableMessageTimestamp )
                stompMessage.NmsTimestamp = DateTime.UtcNow;

            if ( timeToLive != TimeSpan.Zero )
                stompMessage.NmsTimeToLive = timeToLive;

            lock ( _closedLock )
            {
                if ( _closed )
                    throw new ConnectionClosedException();
                _session.DoSend( stompMessage, RequestTimeout );
            }
        }

        public TimeSpan TimeToLive { get; set; } = NmsConstants.DefaultTimeToLive;

        internal void DoClose()
        {
            lock ( _closedLock )
            {
                if ( _closed )
                    return;

                try
                {
                    _session.DisposeOf( _info.ProducerId );
                }
                catch ( Exception ex )
                {
                    Tracer.ErrorFormat( "Error during producer close: {0}", ex );
                }

                _closed = true;
            }
        }

        private void Dispose( Boolean disposing )
        {
            if ( _disposed )
                return;

            if ( disposing )
            {
                // Dispose managed code here.
            }

            try
            {
                Close();
            }
            catch
            {
                // Ignore network errors.
            }

            _disposed = true;
        }

        ~MessageProducer()
        {
            Dispose( false );
        }

        #region Message Creation Factory Methods.
        
        public ITextMessage CreateTextMessage()
            => _session.CreateTextMessage();

        public ITextMessage CreateTextMessage( String text ) 
            => _session.CreateTextMessage( text );

        public IBytesMessage CreateBytesMessage() 
            => _session.CreateBytesMessage();

        public IBytesMessage CreateBytesMessage( Byte[] body ) 
            => _session.CreateBytesMessage( body );

        #endregion
    }
}