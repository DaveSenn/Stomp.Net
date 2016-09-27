#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /*
     *
     *  Command code for OpenWire format for RemoveSubscriptionInfo
     *
     *  NOTE!: This file is auto generated - do not modify!
     *         if you need to make a change, please see the Java Classes
     *         in the nms-activemq-openwire-generator module
     *
     */

    public class RemoveSubscriptionInfo : BaseCommand
    {
        #region Properties

        public ConnectionId ConnectionId { get; set; }

        public String SubscriptionName { get; set; }

        public String ClientId { get; set; }

        /// <summery>
        ///     Return an answer of true to the isRemoveSubscriptionInfo() query.
        /// </summery>
        public override Boolean IsRemoveSubscriptionInfo
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.RemoveSubscriptionInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "ConnectionId=" + ConnectionId + ", " +
                                             "SubscriptionName=" + SubscriptionName + ", " +
                                             "ClientId=" + ClientId +
                                             "]";

        public override Response Visit( ICommandVisitor visitor ) => visitor.ProcessRemoveSubscriptionInfo( this );
    }
}