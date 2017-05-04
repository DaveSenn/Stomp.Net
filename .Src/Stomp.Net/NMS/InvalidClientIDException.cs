#region Usings

using System;

#endregion

namespace Stomp.Net
{
    
    public class InvalidClientIdException : StompException
    {
        #region Ctor

        public InvalidClientIdException( String message )
            : base( message )
        {
        }

        public InvalidClientIdException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public InvalidClientIdException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        public InvalidClientIdException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }
        
        #endregion
    }
}