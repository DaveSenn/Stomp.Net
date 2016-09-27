#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Represents a connection failure.
    /// </summary>
    [Serializable]
    public class NmsConnectionException : NmsException
    {
        #region Ctor

        public NmsConnectionException( String message )
            : base( message )
        {
        }

        public NmsConnectionException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public NmsConnectionException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NmsConnectionException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        /// <summary>
        ///     Initializes a new instance of the NmsConnectionException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected NmsConnectionException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        #endregion
    }
}