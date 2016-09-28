#region Usings

using System;
using Stomp.Net.Util;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class BaseMessage : BaseCommand
    {
        #region Constants

        protected const Int32 DefaultMinimumMessageSize = 1024;

        #endregion

        #region Properties

        public ProducerId ProducerId { get; set; }

        public Destination Destination { get; set; }

        public TransactionId TransactionId { get; set; }

        public MessageId MessageId { get; set; }

        protected TransactionId OriginalTransactionId { get; set; }

        protected String GroupId { get; set; }

        protected Int32 GroupSequence { get; set; }

        public String CorrelationId { get; set; }

        public Boolean Persistent { get; set; }

        public Int64 Expiration { get; set; }

        public Byte Priority { get; set; }

        public Destination ReplyTo { get; set; }

        public Int64 Timestamp { get; set; }

        public String Type { get; set; }

        public Byte[] Content { get; set; }

        public ConsumerId TargetConsumerId { get; set; }

        public Int32 RedeliveryCounter { get; set; }

        public virtual Boolean ReadOnlyBody { get; set; }

        /// <summery>
        ///     Return an answer of true to the isMessage() query.
        /// </summery>
        public override Boolean IsMessage => true;

        #endregion

        /// <summery>
        ///     Clone this object and return a new instance that the caller now owns.
        /// </summery>
        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (Message) base.Clone();

            if ( MessageId != null )
                o.MessageId = (MessageId) MessageId.Clone();

            return o;
        }

        public Boolean IsExpired()
            => Expiration != 0 && DateTime.UtcNow > DateUtils.ToDateTimeUtc( Expiration );

        public void OnMessageRollback()
            => RedeliveryCounter++;

        public void OnSend() => ReadOnlyBody = true;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "ProducerId=" + ProducerId + ", " +
               "Destination=" + Destination + ", " +
               "TransactionId=" + TransactionId + ", " +
               "MessageId=" + MessageId + ", " +
               "OriginalTransactionId=" + OriginalTransactionId + ", " +
               "GroupID=" + GroupId + ", " +
               "GroupSequence=" + GroupSequence + ", " +
               "CorrelationId=" + CorrelationId + ", " +
               "Persistent=" + Persistent + ", " +
               "Expiration=" + Expiration + ", " +
               "Priority=" + Priority + ", " +
               "ReplyTo=" + ReplyTo + ", " +
               "Timestamp=" + Timestamp + ", " +
               "Type=" + Type + ", " +
               "Content=" + Content + ", " +
               "TargetConsumerId=" + TargetConsumerId + ", " +
               "RedeliveryCounter=" + RedeliveryCounter +
               "]";
    }
}