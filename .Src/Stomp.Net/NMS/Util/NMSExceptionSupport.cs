

#region Usings

using System;

#endregion

namespace Apache.NMS.Util
{
    public sealed class NMSExceptionSupport
    {
        #region Ctor

        private NMSExceptionSupport()
        {
        }

        #endregion

        public static NMSException Create( String message, String errorCode, Exception cause )
        {
            var exception = new NMSException( message, errorCode, cause );
            return exception;
        }

        public static NMSException Create( String message, Exception cause )
        {
            var exception = new NMSException( message, cause );
            return exception;
        }

        public static NMSException Create( Exception cause )
        {
            if ( cause is NMSException )
                return (NMSException) cause;
            var msg = cause.Message;
            if ( msg == null || msg.Length == 0 )
                msg = cause.ToString();
            var exception = new NMSException( msg, cause );
            return exception;
        }

        public static MessageEOFException CreateMessageEOFException( Exception cause )
        {
            var msg = cause.Message;
            if ( msg == null || msg.Length == 0 )
                msg = cause.ToString();
            var exception = new MessageEOFException( msg, cause );
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