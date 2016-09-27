#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Apache.NMS
{
    [Serializable]
    public class MessageEOFException : NMSException
    {
        #region Ctor

        public MessageEOFException()
        {
        }

        public MessageEOFException( String message )
            : base( message )
        {
        }

        public MessageEOFException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public MessageEOFException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        public MessageEOFException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        #region ISerializable interface implementation

#if !NETCF

        /// <summary>
        ///     Initializes a new instance of the MessageEOFException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MessageEOFException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

#endif

        #endregion

        #endregion
    }
}