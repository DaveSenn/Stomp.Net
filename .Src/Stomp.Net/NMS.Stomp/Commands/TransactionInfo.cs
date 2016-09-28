#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class TransactionInfo : BaseCommand
    {
        #region Constants

        private const Byte Begin = 0;
        private const Byte Commit = 1;
        private const Byte Rollback = 2;

        #endregion

        #region Properties

        public ConnectionId ConnectionId { get; set; }

        public TransactionId TransactionId { get; set; }

        public Byte Type { get; set; }

        #endregion

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
    }
}