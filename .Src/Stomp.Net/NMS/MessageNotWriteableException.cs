#region Usings

using System;

#endregion

namespace Stomp.Net
{
    
    public class MessageNotWriteableException : StompException
    {
        #region Ctor

        public MessageNotWriteableException( String message )
            : base( message )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageNotWriteableException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageNotWriteableException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageNotWriteableException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }
        
        #endregion
    }
}