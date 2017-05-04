#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     An attempt is made to access an invalid destination
    /// </summary>
    
    public class InvalidDestinationException : StompException
    {
        #region Ctor

        public InvalidDestinationException( String message )
            : base( message )
        {
        }

        public InvalidDestinationException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public InvalidDestinationException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        public InvalidDestinationException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        #endregion
    }
}