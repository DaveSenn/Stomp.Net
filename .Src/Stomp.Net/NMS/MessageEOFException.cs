#region Usings

using System;

#endregion

namespace Stomp.Net
{
    
    public class MessageEofException : StompException
    {
        #region Ctor

        // ReSharper disable once UnusedMember.Global
        public MessageEofException( String message )
            : base( message )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageEofException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public MessageEofException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public MessageEofException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }
        
        #endregion
    }
}