#region Usings

using System;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp
{
    public class StompMessageTransformation : MessageTransformation
    {
        #region Fields

        private readonly Connection connection;

        #endregion

        #region Ctor

        public StompMessageTransformation( Connection connection )
        {
            this.connection = connection;
        }

        #endregion

        #region Creation Methods and Conversion Support Methods

        protected override IMessage DoCreateMessage()
        {
            var message = new Message();
            message.Connection = connection;
            return message;
        }

        protected override IBytesMessage DoCreateBytesMessage()
        {
            var message = new BytesMessage();
            message.Connection = connection;
            return message;
        }

        protected override ITextMessage DoCreateTextMessage()
        {
            var message = new TextMessage();
            message.Connection = connection;
            return message;
        }

        protected override IStreamMessage DoCreateStreamMessage()
        {
            throw new NotSupportedException( "Stomp Cannot process Stream Messages" );
        }

        protected override IMapMessage DoCreateMapMessage()
        {
            var message = new MapMessage();
            message.Connection = connection;
            return message;
        }

        protected override IObjectMessage DoCreateObjectMessage()
        {
            throw new NotSupportedException( "Stomp Cannot process Object Messages" );
        }

        protected override IDestination DoTransformDestination( IDestination destination ) => Destination.Transform( destination );

        protected override void DoPostProcessMessage( IMessage message )
        {
        }

        #endregion
    }
}