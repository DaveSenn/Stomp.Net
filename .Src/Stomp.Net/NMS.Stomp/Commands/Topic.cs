#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Summary description for Topic.
    /// </summary>
    public class Topic : Destination, ITopic
    {
        #region Ctor

        public Topic()
        {
        }

        public Topic( String name )
            : base( name )
        {
        }

        #endregion

        override public DestinationType DestinationType => DestinationType.Topic;

        public String TopicName => PhysicalName;

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (Topic) base.Clone();

            // Now do the deep work required
            // If any new variables are added then this routine will
            // likely need updating

            return o;
        }

        public override Destination CreateDestination( String name ) => new Topic( name );

        public override Byte GetDataStructureType() => DataStructureTypes.TopicType;

        public override Int32 GetDestinationType() => STOMP_TOPIC;
    }
}