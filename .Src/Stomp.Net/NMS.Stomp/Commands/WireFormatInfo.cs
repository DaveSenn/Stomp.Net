#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class WireFormatInfo : BaseCommand
    {
        #region Properties

        public Int64 WriteCheckInterval { get; set; } = 0;

        public Int64 ReadCheckInterval { get; set; } = 0;

        public Single Version { get; set; } = 1.0f;

        public String Session { get; set; }

        /// <summery>
        ///     Return an answer of true to the IsWireFormatInfo() query.
        /// </summery>
        public override Boolean IsWireFormatInfo
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.TransactionInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "WriteCheckInterval=" + WriteCheckInterval + ", " +
                                             "ReadCheckInterval=" + ReadCheckInterval + ", " +
                                             "Session=" + Session + ", " +
                                             "Version=" + Version +
                                             "]";

        public override Response Visit( ICommandVisitor visitor ) => null;
    }
}