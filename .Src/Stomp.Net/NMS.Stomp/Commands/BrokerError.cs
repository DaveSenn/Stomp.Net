

#region Usings

using System;
using System.IO;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public struct StackTraceElement
    {
        public String ClassName;
        public String FileName;
        public String MethodName;
        public Int32 LineNumber;
    }

    /// <summary>
    ///     Represents an exception on the broker
    /// </summary>
    [Serializable]
    public class BrokerError : BaseCommand
    {
        #region Fields

        private BrokerError cause;
        private String exceptionClass;
        private String message;
        private StackTraceElement[] stackTraceElements = { };

        #endregion

        #region Properties

        public String Message
        {
            get { return message; }
            set { message = value; }
        }

        public String ExceptionClass
        {
            get { return exceptionClass; }
            set { exceptionClass = value; }
        }

        public StackTraceElement[] StackTraceElements
        {
            get { return stackTraceElements; }
            set { stackTraceElements = value; }
        }

        public BrokerError Cause
        {
            get { return cause; }
            set { cause = value; }
        }

        public String StackTrace
        {
            get
            {
                var writer = new StringWriter();
                PrintStackTrace( writer );
                return writer.ToString();
            }
        }

        #endregion

        public override Byte GetDataStructureType() => DataStructureTypes.ErrorType;

        public void PrintStackTrace( TextWriter writer )
        {
            writer.WriteLine( exceptionClass + ": " + message );
            for ( var i = 0; i < stackTraceElements.Length; i++ )
            {
                var element = stackTraceElements[i];
                writer.WriteLine( "    at " + element.ClassName + "." + element.MethodName + "(" + element.FileName + ":" + element.LineNumber + ")" );
            }

            if ( cause != null )
            {
                writer.WriteLine( "Nested Exception:" );
                cause.PrintStackTrace( writer );
            }
        }
    }
}