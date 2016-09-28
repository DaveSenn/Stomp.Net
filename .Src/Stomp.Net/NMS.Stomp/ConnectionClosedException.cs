#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Exception thrown when a connection is used that it already closed
    /// </summary>
    [Serializable]
    public class ConnectionClosedException : NmsException
    {
        #region Ctor

        public ConnectionClosedException()
            : base( "The connection is already closed!" )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message )
            : base( message )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        /// <summary>
        ///     Initializes a new instance of the ConnectionClosedException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected ConnectionClosedException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        #endregion
    }
}