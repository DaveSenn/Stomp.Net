#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class TransactionInfo : BaseCommand
    {
        #region Constants

        public const Byte BEGIN = 0;
        public const Byte COMMIT = 1;
        public const Byte ROLLBACK = 2;

        #endregion

        #region Properties

        public ConnectionId ConnectionId { get; set; }

        public TransactionId TransactionId { get; set; }

        public Byte Type { get; set; }

        /// <summery>
        ///     Return an answer of true to the isTransactionInfo() query.
        /// </summery>
        public override Boolean IsTransactionInfo
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
                                             "ConnectionId=" + ConnectionId + ", " +
                                             "TransactionId=" + TransactionId + ", " +
                                             "Type=" + Type +
                                             "]";

        public override Response visit( ICommandVisitor visitor )
        {
            switch ( Type )
            {
                case BEGIN:
                    return visitor.processBeginTransaction( this );
                case COMMIT:
                    return visitor.processCommitTransaction( this );
                case ROLLBACK:
                    return visitor.processRollbackTransaction( this );
                default:
                    throw new IOException( "Transaction info type unknown: " + Type );
            }
        }
    }
}