#region Usings

using System;
using System.Text;
using Extend;
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

        #region Overrides of Exception

        /// <summary>
        ///     Gets a message that describes the current exception.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string ("").</returns>
        public override String Message
            => "{1}{0}BrokerError: {2}".F( Environment.NewLine,
                                           base.Message,
                                           BrokerError );

        #endregion

        #endregion

        #region Ctor

        public BrokerException()
            : base( "Broker failed with missing exception log" )
        {
        }

        public BrokerException( BrokerError brokerError, Exception innerException = null )
            : base( brokerError.ExceptionClass + " : " + brokerError.Message + "\n" + StackTraceDump( brokerError.StackTraceElements ),
                    innerException )
            => BrokerError = brokerError;

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