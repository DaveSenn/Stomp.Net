#region Usings

using System;

#endregion

namespace Stomp.Net
{
    public class IllegalStateException : StompException
    {
        #region Ctor

        public IllegalStateException( String message )
            : base( message )
        {
        }

        public IllegalStateException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public IllegalStateException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        public IllegalStateException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        #endregion
    }
}