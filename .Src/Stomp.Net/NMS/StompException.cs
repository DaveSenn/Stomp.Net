#region Usings

using System;
using Extend;

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

        #region Overrides of Exception

        /// <summary>
        ///     Gets a message that describes the current exception.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string ("").</returns>
        public override String Message
            => "{1}{0}ErrorCode: {2}".F( Environment.NewLine,
                                         base.Message,
                                         ErrorCode );

        #endregion

        #endregion

        #region Ctor

        public StompException( String message )
            : base( message )
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public StompException( String message, String errorCode )
            : this( message )
            => ErrorCode = errorCode;

        public StompException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public StompException( String message, String errorCode, Exception innerException )
            : base( message, innerException )
            => ErrorCode = errorCode;

        #endregion
    }
}