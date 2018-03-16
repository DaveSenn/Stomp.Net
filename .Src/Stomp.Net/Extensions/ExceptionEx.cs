#region Usings

using System;
using Extend;

#endregion

namespace Stomp.Net.Util
{
    public static class ExceptionEx
    {
        public static StompException Create( this Exception cause )
        {
            if ( cause is StompException exception )
                return exception;

            var msg = cause.Message;
            if ( msg.IsEmpty() )
                msg = cause.ToString();

            return new StompException( msg, cause );
        }
    }
}