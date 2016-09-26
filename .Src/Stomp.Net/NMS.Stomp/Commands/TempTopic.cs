#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     A Temporary Topic
    /// </summary>
    public class TempTopic : TempDestination, ITemporaryTopic
    {
        #region Ctor

        public TempTopic()
        {
        }

        public TempTopic( String name )
            : base( name )
        {
        }

        #endregion

        override public DestinationType DestinationType
        {
            get { return DestinationType.TemporaryTopic; }
        }

        public String TopicName
        {
            get { return PhysicalName; }
        }

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

        public override Byte GetDataStructureType() => DataStructureTypes.TempTopicType;

        public override Int32 GetDestinationType() => STOMP_TEMPORARY_TOPIC;
    }
}