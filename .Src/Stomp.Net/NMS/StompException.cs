#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Represents an NMS exception
    /// </summary>
    public class StompException : Exception
    {
        #region Properties

        /// <summary>
        ///     Returns the error code for the exception, if one has been provided.
        /// </summary>
        private String ErrorCode { get; }

        #endregion

        #region Ctor

        public StompException( String message )
            : base( message )
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public StompException( String message, String errorCode )
            : this( message )
        {
            ErrorCode = errorCode;
        }

        public StompException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public StompException( String message, String errorCode, Exception innerException )
            : base( message, innerException )
        {
            ErrorCode = errorCode;
        }

        #endregion
        
    }
}