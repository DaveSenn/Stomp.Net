#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Represents an NMS exception
    /// </summary>
    [Serializable]
    public class NMSException : Exception
    {
        #region Fields

        protected String exceptionErrorCode;

        #endregion

        #region Properties

        /// <summary>
        ///     Returns the error code for the exception, if one has been provided.
        /// </summary>
        public String ErrorCode
        {
            get { return exceptionErrorCode; }
        }

        #endregion

        #region Ctor

        public NMSException()
        {
        }

        public NMSException( String message )
            : base( message )
        {
        }

        public NMSException( String message, String errorCode )
            : this( message )
        {
            exceptionErrorCode = errorCode;
        }

        public NMSException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        public NMSException( String message, String errorCode, Exception innerException )
            : base( message, innerException )
        {
            exceptionErrorCode = errorCode;
        }

        #endregion

        #region ISerializable interface implementation

#if !NETCF

        /// <summary>
        ///     Initializes a new instance of the NMSException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected NMSException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            exceptionErrorCode = info.GetString( "NMSException.exceptionErrorCode" );
        }

        /// <summary>
        ///     When overridden in a derived class, sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
            info.AddValue( "NMSException.exceptionErrorCode", exceptionErrorCode );
        }

#endif

        #endregion
    }
}