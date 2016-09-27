#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class MessageAck : BaseCommand
    {
        #region Properties

        public Destination Destination { get; set; }

        public TransactionId TransactionId { get; set; }

        public ConsumerId ConsumerId { get; set; }

        public Byte AckType { get; set; }

        public MessageId FirstMessageId { get; set; }

        public MessageId LastMessageId { get; set; }

        public Int32 MessageCount { get; set; }

        /// <summery>
        ///     Return an answer of true to the isMessageAck() query.
        /// </summery>
        public override Boolean IsMessageAck
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.MessageAckType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "Destination=" + Destination + ", " +
                                             "TransactionId=" + TransactionId + ", " +
                                             "ConsumerId=" + ConsumerId + ", " +
                                             "AckType=" + AckType + ", " +
                                             "FirstMessageId=" + FirstMessageId + ", " +
                                             "LastMessageId=" + LastMessageId + ", " +
                                             "MessageCount=" + MessageCount +
                                             "]";

        public override Response Visit( ICommandVisitor visitor ) => visitor.ProcessMessageAck( this );
    }
}