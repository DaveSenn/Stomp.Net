#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     Command code for OpenWire format for RemoveSubscriptionInfo
    /// </summary>
    public class RemoveSubscriptionInfo : BaseCommand
    {
        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.RemoveSubscriptionInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "ConnectionId=" + ConnectionId + ", " +
               "SubscriptionName=" + SubscriptionName + ", " +
               "ClientId=" + ClientId +
               "]";

        #region Properties

        public ConnectionId ConnectionId { get; set; }

        public String SubscriptionName { get; set; }

        public String ClientId { get; set; }

        /// <summery>
        ///     Return an answer of true to the isRemoveSubscriptionInfo() query.
        /// </summery>
        public override Boolean IsRemoveSubscriptionInfo => true;

        #endregion
    }
}