#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Represents a security failure.
    /// </summary>
    [Serializable]
    public class NMSSecurityException : NMSException
    {
        #region Ctor

        public NMSSecurityException()
        {
        }

        public NMSSecurityException( String message )
            : base( message )
        {
        }

        public NMSSecurityException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public NMSSecurityException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        public NMSSecurityException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        #region ISerializable interface implementation

#if !NETCF

        /// <summary>
        ///     Initializes a new instance of the NMSSecurityException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected NMSSecurityException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

#endif

        #endregion

        #endregion
    }
}