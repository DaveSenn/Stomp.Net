#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Represents a connection failure.
    /// </summary>
    public class StompConnectionException : StompException
    {
        #region Ctor

        public StompConnectionException( String message )
            : base( message )
        {
        }

        public StompConnectionException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public StompConnectionException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public StompConnectionException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        #endregion
    }
}