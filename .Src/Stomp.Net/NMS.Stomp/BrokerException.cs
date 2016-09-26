#region Usings

using System;
using System.Runtime.Serialization;
using System.Text;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Exception thrown when the broker returns an error
    /// </summary>
    [Serializable]
    public class BrokerException : NMSException
    {
        #region Properties

        public BrokerError BrokerError { get; }

        public virtual String JavaStackTrace
        {
            get { return BrokerError.StackTrace; }
        }

        #endregion

        #region Ctor

        public BrokerException()
            : base( "Broker failed with missing exception log" )
        {
        }

        public BrokerException( BrokerError brokerError )
            : this( brokerError, null )
        {
        }

        public BrokerException( BrokerError brokerError, Exception innerException )
            : base( brokerError.ExceptionClass + " : " + brokerError.Message + "\n" + StackTraceDump( brokerError.StackTraceElements ),
                    innerException )
        {
            BrokerError = brokerError;
        }

        #endregion

        /// <summary>
        ///     Generates a nice textual stack trace
        /// </summary>
        public static String StackTraceDump( StackTraceElement[] elements )
        {
            var builder = new StringBuilder();
            if ( elements != null )
                foreach ( var e in elements )
                    builder.Append( "\n " + e.ClassName + "." + e.MethodName + "(" + e.FileName + ":" + e.LineNumber + ")" );
            return builder.ToString();
        }

        #region ISerializable interface implementation

#if !NETCF

        /// <summary>
        ///     Initializes a new instance of the BrokerException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected BrokerException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            BrokerError = info.GetValue( "BrokerException.brokerError", typeof(BrokerError) ) as BrokerError;
        }

        /// <summary>
        ///     When overridden in a derived class, sets the SerializationInfo
        ///     with information about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
            info.AddValue( "BrokerException.brokerError", BrokerError );
        }

#endif

        #endregion
    }
}