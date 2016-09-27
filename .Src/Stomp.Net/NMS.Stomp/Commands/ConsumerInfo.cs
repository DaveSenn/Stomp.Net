#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ConsumerInfo : BaseCommand
    {
        #region Constants

        private const Byte IdConsumerinfo = 5;

        #endregion

        #region Properties

        public ConsumerId ConsumerId { get; set; }

        public Destination Destination { get; set; }

        public AcknowledgementMode AckMode { get; set; }

        public Int32 PrefetchSize { get; set; }

        public Int32 MaximumPendingMessageLimit { get; set; }

        public Boolean DispatchAsync { get; set; }

        public String Selector { get; set; }

        public String SubscriptionName { get; set; }

        public Boolean NoLocal { get; set; }

        public Boolean Exclusive { get; set; }

        public Boolean Retroactive { get; set; }

        public Byte Priority { get; set; }

        public String Transformation { get; set; }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => IdConsumerinfo;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "ConsumerId=" + ConsumerId + ", " +
                                             "Destination=" + Destination + ", " +
                                             "Ack Mode=" + AckMode + ", " +
                                             "PrefetchSize=" + PrefetchSize + ", " +
                                             "MaximumPendingMessageLimit=" + MaximumPendingMessageLimit + ", " +
                                             "DispatchAsync=" + DispatchAsync + ", " +
                                             "Selector=" + Selector + ", " +
                                             "SubscriptionName=" + SubscriptionName + ", " +
                                             "NoLocal=" + NoLocal + ", " +
                                             "Exclusive=" + Exclusive + ", " +
                                             "Retroactive=" + Retroactive + ", " +
                                             "Priority=" + Priority + ", " +
                                             "Transformation" + Transformation +
                                             "]";

        public override Response Visit( ICommandVisitor visitor ) => visitor.ProcessAddConsumer( this );
    }
}