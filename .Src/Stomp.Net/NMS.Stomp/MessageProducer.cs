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

        private readonly Object closedLock = new Object();
        private readonly ProducerInfo info;

        private readonly MessageTransformation messageTransformation;
        private Boolean closed;
        protected Boolean disposed;

        private Int32 producerSequenceId;

        private Session session;

        #endregion

        #region Properties

        public ProducerId ProducerId
        {
            get { return info.ProducerId; }
        }

        #endregion

        #region Ctor

        public MessageProducer( Session session, ProducerInfo info )
        {
            this.session = session;
            this.info = info;
            RequestTimeout = session.RequestTimeout;
            messageTransformation = session.Connection.MessageTransformation;
        }

        #endregion

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void Close()
        {
            lock ( closedLock )
            {
                if ( closed )
                    return;

                DoClose();
                session = null;
            }
        }

        public MsgDeliveryMode DeliveryMode { get; set; } = NmsConstants.defaultDeliveryMode;

        public Boolean DisableMessageID { get; set; } = false;

        public Boolean DisableMessageTimestamp { get; set; } = false;

        public MsgPriority Priority { get; set; } = NmsConstants.defaultPriority;

        public ProducerTransformerDelegate ProducerTransformer { get; set; }

        public TimeSpan RequestTimeout { get; set; }

        public void Send( IMessage message ) => Send( info.Destination, message, DeliveryMode, Priority, TimeToLive );

        public void Send( IDestination destination, IMessage message ) => Send( destination, message, DeliveryMode, Priority, TimeToLive );

        public void Send( IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive )
            => Send( info.Destination, message, deliveryMode, priority, timeToLive );

        public void Send( IDestination destination, IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive )
        {
            if ( null == destination )
            {
                // See if this producer was created without a destination.
                if ( null == info.Destination )
                    throw new NotSupportedException();

                // The producer was created with a destination, but an invalid destination
                // was specified.
                throw new InvalidDestinationException();
            }

            Destination dest = null;

            if ( destination == info.Destination )
                dest = destination as Destination;
            else if ( info.Destination == null )
                dest = Destination.Transform( destination );
            else
                throw new NotSupportedException( "This producer can only send messages to: " + info.Destination.PhysicalName );

            if ( ProducerTransformer != null )
            {
                var transformed = ProducerTransformer( session, this, message );
                if ( transformed != null )
                    message = transformed;
            }

            var stompMessage = messageTransformation.TransformMessage<Message>( message );

            stompMessage.ProducerId = info.ProducerId;
            stompMessage.FromDestination = dest;
            stompMessage.NMSDeliveryMode = deliveryMode;
            stompMessage.NMSPriority = priority;

            // Always set the message Id regardless of the disable flag.
            var id = new MessageId();
            id.ProducerId = info.ProducerId;
            id.ProducerSequenceId = Interlocked.Increment( ref producerSequenceId );
            stompMessage.MessageId = id;

            if ( !DisableMessageTimestamp )
                stompMessage.NMSTimestamp = DateTime.UtcNow;

            if ( timeToLive != TimeSpan.Zero )
                stompMessage.NMSTimeToLive = timeToLive;

            lock ( closedLock )
            {
                if ( closed )
                    throw new ConnectionClosedException();
                session.DoSend( stompMessage, this, RequestTimeout );
            }
        }

        public TimeSpan TimeToLive { get; set; } = NmsConstants.defaultTimeToLive;

        protected void Dispose( Boolean disposing )
        {
            if ( disposed )
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

            disposed = true;
        }

        internal void DoClose()
        {
            lock ( closedLock )
            {
                if ( closed )
                    return;

                try
                {
                    session.DisposeOf( info.ProducerId );
                }
                catch ( Exception ex )
                {
                    Tracer.ErrorFormat( "Error during producer close: {0}", ex );
                }

                closed = true;
            }
        }

        ~MessageProducer()
        {
            Dispose( false );
        }

        #region Message Creation Factory Methods.

        public IMessage CreateMessage() => session.CreateMessage();

        public ITextMessage CreateTextMessage() => session.CreateTextMessage();

        public ITextMessage CreateTextMessage( String text ) => session.CreateTextMessage( text );

        public IMapMessage CreateMapMessage() => session.CreateMapMessage();

        public IObjectMessage CreateObjectMessage( Object body )
        {
            throw new NotSupportedException( "No Object Message in Stomp" );
        }

        public IBytesMessage CreateBytesMessage() => session.CreateBytesMessage();

        public IBytesMessage CreateBytesMessage( Byte[] body ) => session.CreateBytesMessage( body );

        public IStreamMessage CreateStreamMessage() => session.CreateStreamMessage();

        #endregion
    }
}