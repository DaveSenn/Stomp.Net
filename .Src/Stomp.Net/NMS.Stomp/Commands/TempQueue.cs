#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     A Temporary Queue
    /// </summary>
    public class TempQueue : TempDestination, ITemporaryQueue
    {
        #region Ctor
        

        public TempQueue( String name )
            : base( name )
        {
        }

        #endregion

        public override DestinationType DestinationType
            => DestinationType.TemporaryQueue;

        public String QueueName
            => PhysicalName;

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (TempQueue) base.Clone();

            // Now do the deep work required
            // If any new variables are added then this routine will
            // likely need updating

            return o;
        }

        public override Destination CreateDestination( String name )
            => new TempQueue( name );

        public override Byte GetDataStructureType()
            => DataStructureTypes.TempQueueType;

        protected override Int32 GetDestinationType()
            => StompTemporaryQueue;
    }
}