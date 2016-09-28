#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Represents an NMS exception
    /// </summary>
    [Serializable]
    public class NmsException : Exception
    {
        #region Properties

        /// <summary>
        ///     Returns the error code for the exception, if one has been provided.
        /// </summary>
        private String ErrorCode { get; }

        #endregion

        #region Ctor

        public NmsException( String message )
            : base( message )
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public NmsException( String message, String errorCode )
            : this( message )
        {
            ErrorCode = errorCode;
        }

        public NmsException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public NmsException( String message, String errorCode, Exception innerException )
            : base( message, innerException )
        {
            ErrorCode = errorCode;
        }

        #endregion

        #region ISerializable interface implementation

        /// <summary>
        ///     Initializes a new instance of the NmsException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected NmsException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            ErrorCode = info.GetString( "NmsException.exceptionErrorCode" );
        }

        /// <summary>
        ///     When overridden in a derived class, sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
            info.AddValue( "NmsException.exceptionErrorCode", ErrorCode );
        }

        #endregion
    }
}