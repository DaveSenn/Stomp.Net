#region Usings

using System;
using Apache.NMS.Stomp.Protocol;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class TextMessage : Message, ITextMessage
    {
        #region Fields

        private String _text;

        #endregion

        #region Ctor

        public TextMessage()
        {
        }

        public TextMessage( String text )
        {
            Text = text;
        }

        #endregion

        public override void ClearBody()
        {
            base.ClearBody();
            _text = null;
        }

        public String Text
        {
            get { return _text; }

            set
            {
                FailIfReadOnlyBody();
                _text = value;
                Content = null;
            }
        }

        public override void BeforeMarshall( StompWireFormat wireFormat )
        {
            base.BeforeMarshall( wireFormat );

            if ( Content == null && _text != null )
            {
                Content = wireFormat.Encoder.GetBytes( _text );
                _text = null;
            }
        }

        public override Byte GetDataStructureType() => DataStructureTypes.TextMessageType;

        public override String ToString()
        {
            var text = Text;

            if ( text != null && text.Length > 63 )
                text = text.Substring( 0, 45 ) + "..." + text.Substring( text.Length - 12 );
            return base.ToString() + " Text = " + ( text ?? "null" );
        }

        protected override Int32 Size()
        {
            if ( Content == null && _text != null )
            {
                var size = DefaultMinimumMessageSize;

                if ( MarshalledProperties != null )
                    size += MarshalledProperties.Length;

                return size += _text.Length * 2;
            }

            return base.Size();
        }
    }
}