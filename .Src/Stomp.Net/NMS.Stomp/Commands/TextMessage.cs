

#region Usings

using System;
using Apache.NMS.Stomp.Protocol;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class TextMessage : Message, ITextMessage
    {
        #region Fields

        private String text;

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
            text = null;
        }

        // Properties

        public String Text
        {
            get { return text; }

            set
            {
                FailIfReadOnlyBody();
                text = value;
                Content = null;
            }
        }

        public override void BeforeMarshall( StompWireFormat wireFormat )
        {
            base.BeforeMarshall( wireFormat );

            if ( Content == null && text != null )
            {
                Content = wireFormat.Encoder.GetBytes( text );
                text = null;
            }
        }

        public override Byte GetDataStructureType()
        {
            return DataStructureTypes.TextMessageType;
        }

        public override Int32 Size()
        {
            if ( Content == null && text != null )
            {
                var size = DEFAULT_MINIMUM_MESSAGE_SIZE;

                if ( MarshalledProperties != null )
                    size += MarshalledProperties.Length;

                return size += text.Length * 2;
            }

            return base.Size();
        }

        public override String ToString()
        {
            var text = Text;

            if ( text != null && text.Length > 63 )
                text = text.Substring( 0, 45 ) + "..." + text.Substring( text.Length - 12 );
            return base.ToString() + " Text = " + ( text ?? "null" );
        }
    }
}