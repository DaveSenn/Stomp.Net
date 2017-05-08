#region Usings

using System;
using System.Text;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Exception thrown when the broker returns an error
    /// </summary>
    public class BrokerException : StompException
    {
        #region Properties

        private BrokerError BrokerError { get; }

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
        private static String StackTraceDump( StackTraceElement[] elements )
        {
            var builder = new StringBuilder();
            if ( elements == null )
                return builder.ToString();

            foreach ( var e in elements )
                builder.Append( "\n " + e.ClassName + "." + e.MethodName + "(" + e.FileName + ":" + e.LineNumber + ")" );
            return builder.ToString();
        }
    }
}