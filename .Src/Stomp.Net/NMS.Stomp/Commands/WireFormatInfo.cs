#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class WireFormatInfo : BaseCommand
    {
        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.TransactionInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "WriteCheckInterval=" + WriteCheckInterval + ", " +
               "ReadCheckInterval=" + ReadCheckInterval + ", " +
               "Session=" + Session + ", " +
               "Version=" + Version +
               "]";

        #region Properties

        public Int32 WriteCheckInterval { get; set; }

        public Int32 ReadCheckInterval { get; set; }

        public Single Version { get; set; } = 1.0f;

        public String Session { get; set; }

        /// <summery>
        ///     Return an answer of true to the IsWireFormatInfo() query.
        /// </summery>
        public override Boolean IsWireFormatInfo => true;

        #endregion
    }
}