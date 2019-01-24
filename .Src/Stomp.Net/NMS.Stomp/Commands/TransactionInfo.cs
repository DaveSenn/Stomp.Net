#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class TransactionInfo : BaseCommand
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
               "ConnectionId=" + ConnectionId + ", " +
               "TransactionId=" + TransactionId + ", " +
               "Type=" + Type +
               "]";

        #region Properties

        public ConnectionId ConnectionId { get; set; }

        public TransactionId TransactionId { get; set; }

        public Byte Type { get; set; }

        #endregion
    }
}