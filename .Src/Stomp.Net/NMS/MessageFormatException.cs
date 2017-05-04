#region Usings

using System;

#endregion

namespace Stomp.Net
{
    
    public class MessageFormatException : StompException
    {
        #region Ctor

        public MessageFormatException( String message )
            : base( message )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageFormatException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public MessageFormatException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageFormatException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }
        
        #endregion
    }
}