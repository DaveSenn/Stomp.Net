#region Usings

using Apache.NMS.Stomp.Commands;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp
{
    public class StompMessageTransformation : MessageTransformation
    {
        #region Fields

        private readonly Connection _connection;

        #endregion

        #region Ctor

        public StompMessageTransformation( Connection connection )
        {
            _connection = connection;
        }

        #endregion

        #region Creation Methods and Conversion Support Methods

        protected override IMessage DoCreateMessage()
        {
            var message = new Message { Connection = _connection };
            return message;
        }

        protected override IBytesMessage DoCreateBytesMessage()
        {
            var message = new BytesMessage { Connection = _connection };
            return message;
        }

        protected override ITextMessage DoCreateTextMessage()
        {
            var message = new TextMessage { Connection = _connection };
            return message;
        }
        
        protected override IDestination DoTransformDestination( IDestination destination )
            => Destination.Transform( destination );

        protected override void DoPostProcessMessage( IMessage message )
        {
        }

        #endregion
    }
}