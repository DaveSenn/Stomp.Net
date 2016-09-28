#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     A Temporary Topic
    /// </summary>
    public class TempTopic : TempDestination, ITemporaryTopic
    {
        #region Ctor

        public TempTopic( String name )
            : base( name )
        {
        }

        #endregion

        public override DestinationType DestinationType
            => DestinationType.TemporaryTopic;

        public String TopicName
            => PhysicalName;

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (TempTopic) base.Clone();

            // Now do the deep work required
            // If any new variables are added then this routine will
            // likely need updating

            return o;
        }

        public override Destination CreateDestination( String name ) => new TempTopic( name );

        public override Byte GetDataStructureType()
            => DataStructureTypes.TempTopicType;

        protected override Int32 GetDestinationType()
            => StompTemporaryTopic;
    }
}