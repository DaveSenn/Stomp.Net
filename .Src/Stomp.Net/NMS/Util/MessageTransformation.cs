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
        public T TransformMessage<T>( IMessage message )
        {
            if ( message is T )
                return (T) message;
            IMessage result;

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

        /// <summary>
        ///     Copies the standard NMS and user defined properties from the given
        ///     message to the specified message, the class version transforms the
        ///     Destination instead of just doing a straight copy.
        /// </summary>
        private void CopyProperties( IMessage fromMessage, IMessage toMessage )
        {
            toMessage.NmsMessageId = fromMessage.NmsMessageId;
            toMessage.NmsCorrelationId = fromMessage.NmsCorrelationId;
            toMessage.NmsReplyTo = DoTransformDestination( fromMessage.NmsReplyTo );
            toMessage.NmsDestination = DoTransformDestination( fromMessage.NmsDestination );
            toMessage.NmsDeliveryMode = fromMessage.NmsDeliveryMode;
            toMessage.NmsRedelivered = fromMessage.NmsRedelivered;
            toMessage.NmsType = fromMessage.NmsType;
            toMessage.NmsPriority = fromMessage.NmsPriority;
            toMessage.NmsTimestamp = fromMessage.NmsTimestamp;
            toMessage.NmsTimeToLive = fromMessage.NmsTimeToLive;

            foreach ( var x in fromMessage.Headers )
                toMessage.Headers[x.Key] = fromMessage.Headers[x.Value];
        }

        #region Creation Methods and Conversion Support Methods

        protected abstract IMessage DoCreateMessage();
        protected abstract IBytesMessage DoCreateBytesMessage();
        protected abstract ITextMessage DoCreateTextMessage();
        
        protected abstract IDestination DoTransformDestination( IDestination destination );
        protected abstract void DoPostProcessMessage( IMessage message );

        #endregion
    }
}