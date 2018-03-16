#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     Summary description for Queue.
    /// </summary>
    public class Queue : Destination, IQueue
    {
        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Queue" /> class with the given physical name.
        /// </summary>
        /// <param name="name">The physical name of the destination.</param>
        /// <param name="skipDesinationNameFormatting">
        ///     A value indicating whether the destination name formatting will be skipped
        ///     or not.
        /// </param>
        public Queue( String name, Boolean skipDesinationNameFormatting )
            : base( name, skipDesinationNameFormatting )
        {
        }

        #endregion

        #region Public Members

        public override DestinationType DestinationType
            => DestinationType.Queue;

        #endregion

        #region Implementation of IQueue

        public String QueueName
            => PhysicalName;

        #endregion

        #region Overrides

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (Queue) base.Clone();

            // Now do the deep work required
            // If any new variables are added then this routine will
            // likely need updating
            return o;
        }

        [PublicAPI]
        public virtual Destination CreateDestination( String name )
            => new Queue( name, SkipDesinationNameFormatting );

        public override Byte GetDataStructureType()
            => DataStructureTypes.QueueType;

        protected override Int32 GetDestinationType()
            => StompQueue;

        #endregion
    }
}