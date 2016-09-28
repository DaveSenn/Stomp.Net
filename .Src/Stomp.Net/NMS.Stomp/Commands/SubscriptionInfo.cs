#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class SubscriptionInfo : BaseDataStructure
    {
        #region Properties

        public String ClientId { get; set; }

        public Destination Destination { get; set; }

        public String Selector { get; set; }

        public String SubscriptionName { get; set; }

        public Destination SubscribedDestination { get; set; }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.SubscriptionInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "ClientId=" + ClientId + ", " +
               "Destination=" + Destination + ", " +
               "Selector=" + Selector + ", " +
               "SubscriptionName=" + SubscriptionName + ", " +
               "SubscribedDestination=" + SubscribedDestination +
               "]";
    }
}