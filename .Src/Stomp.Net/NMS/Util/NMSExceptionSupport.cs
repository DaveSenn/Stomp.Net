#region Usings

using System;

#endregion

namespace Apache.NMS.Util
{
    public static class NmsExceptionSupport
    {
        public static NmsException Create( String message, Exception cause )
        {
            var exception = new NmsException( message, cause );
            return exception;
        }

        public static NmsException Create( Exception cause )
        {
            if ( cause is NmsException )
                return (NmsException) cause;
            var msg = cause.Message;
            if ( msg == null || msg.Length == 0 )
                msg = cause.ToString();
            var exception = new NmsException( msg, cause );
            return exception;
        }

        public static MessageEofException CreateMessageEofException( Exception cause )
        {
            var msg = cause.Message;
            if ( msg == null || msg.Length == 0 )
                msg = cause.ToString();
            var exception = new MessageEofException( msg, cause );
            return exception;
        }

        public static MessageFormatException CreateMessageFormatException( Exception cause )
        {
            var msg = cause.Message;
            if ( msg == null || msg.Length == 0 )
                msg = cause.ToString();
            var exception = new MessageFormatException( msg, cause );
            return exception;
        }
    }
}