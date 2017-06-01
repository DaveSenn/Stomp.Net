#region Usings

#endregion

#region Usings

using System;

#endregion

namespace Stomp.Net.Util
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
                var msg = DoCreateBytesMessage();

                try
                {
                    msg.Content = new Byte[bytesMsg.Content.Length];
                    Array.Copy( bytesMsg.Content, msg.Content, bytesMsg.Content.Length );
                }
                catch
                {
                    // ignored
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
                result = DoCreateMessage();

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
            toMessage.StompMessageId = fromMessage.StompMessageId;
            toMessage.StompCorrelationId = fromMessage.StompCorrelationId;
            toMessage.StompReplyTo = DoTransformDestination( fromMessage.StompReplyTo );
            toMessage.StompDestination = DoTransformDestination( fromMessage.StompDestination );
            toMessage.StompDeliveryMode = fromMessage.StompDeliveryMode;
            toMessage.StompRedelivered = fromMessage.StompRedelivered;
            toMessage.StompType = fromMessage.StompType;
            toMessage.StompPriority = fromMessage.StompPriority;
            toMessage.StompTimestamp = fromMessage.StompTimestamp;
            toMessage.StompTimeToLive = fromMessage.StompTimeToLive;

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