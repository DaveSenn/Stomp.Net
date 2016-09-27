#region Usings

using System;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     Base Utility class for conversion between IMessage type objects for different
    ///     NMS providers.
    /// </summary>
    public abstract class MessageTransformation
    {
        /// <summary>
        ///     Copies the standard NMS and user defined properties from the givem
        ///     message to the specified message, this method makes no attempt to convert
        ///     the values in the Message to native provider implementations.
        /// </summary>
        public static void CopyNMSMessageProperties( IMessage fromMessage, IMessage toMessage )
        {
            toMessage.NMSMessageId = fromMessage.NMSMessageId;
            toMessage.NMSCorrelationID = fromMessage.NMSCorrelationID;
            toMessage.NMSReplyTo = fromMessage.NMSReplyTo;
            toMessage.NMSDestination = fromMessage.NMSDestination;
            toMessage.NMSDeliveryMode = fromMessage.NMSDeliveryMode;
            toMessage.NMSRedelivered = fromMessage.NMSRedelivered;
            toMessage.NMSType = fromMessage.NMSType;
            toMessage.NMSPriority = fromMessage.NMSPriority;
            toMessage.NMSTimestamp = fromMessage.NMSTimestamp;
            toMessage.NMSTimeToLive = fromMessage.NMSTimeToLive;

            foreach ( String key in fromMessage.Properties.Keys )
                toMessage.Properties[key] = fromMessage.Properties[key];
        }

        /// <summary>
        ///     Copies the standard NMS and user defined properties from the givem
        ///     message to the specified message, the class version transforms the
        ///     Destination instead of just doing a straight copy.
        /// </summary>
        public virtual void CopyProperties( IMessage fromMessage, IMessage toMessage )
        {
            toMessage.NMSMessageId = fromMessage.NMSMessageId;
            toMessage.NMSCorrelationID = fromMessage.NMSCorrelationID;
            toMessage.NMSReplyTo = DoTransformDestination( fromMessage.NMSReplyTo );
            toMessage.NMSDestination = DoTransformDestination( fromMessage.NMSDestination );
            toMessage.NMSDeliveryMode = fromMessage.NMSDeliveryMode;
            toMessage.NMSRedelivered = fromMessage.NMSRedelivered;
            toMessage.NMSType = fromMessage.NMSType;
            toMessage.NMSPriority = fromMessage.NMSPriority;
            toMessage.NMSTimestamp = fromMessage.NMSTimestamp;
            toMessage.NMSTimeToLive = fromMessage.NMSTimeToLive;

            foreach ( String key in fromMessage.Properties.Keys )
                toMessage.Properties[key] = fromMessage.Properties[key];
        }

        public T TransformMessage<T>( IMessage message )
        {
            if ( message is T )
                return (T) message;
            IMessage result = null;

            if ( message is IBytesMessage )
            {
                var bytesMsg = message as IBytesMessage;
                bytesMsg.Reset();
                var msg = DoCreateBytesMessage();

                try
                {
                    for ( ;; )
                        msg.WriteByte( bytesMsg.ReadByte() );
                }
                catch
                {
                }

                result = msg;
            }
            else if ( message is IMapMessage )
            {
                var mapMsg = message as IMapMessage;
                var msg = DoCreateMapMessage();

                foreach ( String key in mapMsg.Body.Keys )
                    msg.Body[key] = mapMsg.Body[key];

                result = msg;
            }
            else if ( message is IObjectMessage )
            {
                var objMsg = message as IObjectMessage;
                var msg = DoCreateObjectMessage();
                msg.Body = objMsg.Body;

                result = msg;
            }
            else if ( message is IStreamMessage )
            {
                var streamMessage = message as IStreamMessage;
                streamMessage.Reset();
                var msg = DoCreateStreamMessage();

                Object obj = null;

                try
                {
                    while ( ( obj = streamMessage.ReadObject() ) != null )
                        msg.WriteObject( obj );
                }
                catch
                {
                }

                result = msg;
            }
            else if ( message is ITextMessage )
            {
                var textMsg = message as ITextMessage;
                var msg = DoCreateTextMessage();
                msg.Text = textMsg.Text;
                result = msg;
            }
            else
            {
                result = DoCreateMessage();
            }

            CopyProperties( message, result );

            // Let the subclass have a chance to do any last minute configurations
            // on the newly converted message.
            DoPostProcessMessage( result );

            return (T) result;
        }

        #region Creation Methods and Conversion Support Methods

        protected abstract IMessage DoCreateMessage();
        protected abstract IBytesMessage DoCreateBytesMessage();
        protected abstract ITextMessage DoCreateTextMessage();
        protected abstract IStreamMessage DoCreateStreamMessage();
        protected abstract IMapMessage DoCreateMapMessage();
        protected abstract IObjectMessage DoCreateObjectMessage();

        protected abstract IDestination DoTransformDestination( IDestination destination );
        protected abstract void DoPostProcessMessage( IMessage message );

        #endregion
    }
}