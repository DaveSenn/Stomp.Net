#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     Summary description for Topic.
    /// </summary>
    public class Topic : Destination, ITopic
    {
        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Topic" /> class with the given physical name.
        /// </summary>
        /// <param name="name">The physical name of the destination.</param>
        /// <param name="skipDesinationNameFormatting">
        ///     A value indicating whether the destination name formatting will be skipped
        ///     or not.
        /// </param>
        public Topic(String name, Boolean skipDesinationNameFormatting)
            : base( name, skipDesinationNameFormatting )
        {
        }

        #endregion

        public override DestinationType DestinationType
            => DestinationType.Topic;

        public String TopicName
            => PhysicalName;

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

        public virtual Destination CreateDestination( String name )
            => new Topic( name, SkipDesinationNameFormatting );

        public override Byte GetDataStructureType()
            => DataStructureTypes.TopicType;

        protected override Int32 GetDestinationType()
            => StompTopic;
    }
}