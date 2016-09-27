#region Usings

using System;
using Extend;

#endregion

namespace Apache.NMS.Util
{
    public static class ExceptionEx
    {
        public static NmsException Create( this Exception cause )
        {
            var nmsException = cause as NmsException;
            if ( nmsException != null )
                return nmsException;

            var msg = cause.Message;
            if ( msg.IsEmpty() )
                msg = cause.ToString();

            return new NmsException( msg, cause );
        }

        public static MessageEofException CreateMessageEofException( this Exception cause )
        {
            var msg = cause.Message;
            if ( msg.IsEmpty() )
                msg = cause.ToString();

            return new MessageEofException( msg, cause );
        }

        public static MessageFormatException CreateMessageFormatException( this Exception cause )
        {
            var msg = cause.Message;
            if ( msg.IsEmpty() )
                msg = cause.ToString();

            var exception = new MessageFormatException( msg, cause );
            return exception;
        }
    }
}