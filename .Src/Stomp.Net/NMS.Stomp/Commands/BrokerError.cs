#region Usings

using System;
using System.IO;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Represents an exception on the broker
    /// </summary>
    [Serializable]
    public class BrokerError : BaseCommand
    {
        #region Properties

        public String Message { get; set; }

        public String ExceptionClass { get; set; }

        public StackTraceElement[] StackTraceElements { get; set; }

        public BrokerError Cause { get; set; }

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

        private void PrintStackTrace( TextWriter writer )
        {
            writer.WriteLine( ExceptionClass + ": " + Message );
            for ( var i = 0; i < StackTraceElements.Length; i++ )
            {
                var element = StackTraceElements[i];
                writer.WriteLine( "    at " + element.ClassName + "." + element.MethodName + "(" + element.FileName + ":" + element.LineNumber + ")" );
            }

            if ( Cause != null )
            {
                writer.WriteLine( "Nested Exception:" );
                Cause.PrintStackTrace( writer );
            }
        }
    }
}