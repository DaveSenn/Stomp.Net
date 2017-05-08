#region Usings

using System;

#endregion

namespace Stomp.Net
{
    public class MessageNotReadableException : StompException
    {
        #region Ctor

        public MessageNotReadableException( String message )
            : base( message )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageNotReadableException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageNotReadableException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageNotReadableException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        #endregion
    }
}