#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     Represents an exception on the broker
    /// </summary>
    public class BrokerError : BaseCommand
    {
        #region Properties

        public String Message { get; set; }

        public String ExceptionClass { get; set; }

        public StackTraceElement[] StackTraceElements { get; set; }

        public BrokerError Cause { get; set; }

        #endregion

        public override Byte GetDataStructureType() => DataStructureTypes.ErrorType;
    }
}