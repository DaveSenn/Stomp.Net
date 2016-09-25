

#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Summary description for Queue.
    /// </summary>
    public class Queue : Destination, IQueue
    {
        #region Ctor

        public Queue()
        {
        }

        public Queue( String name )
            : base( name )
        {
        }

        #endregion

        override public DestinationType DestinationType
        {
            get { return DestinationType.Queue; }
        }

        public String QueueName
        {
            get { return PhysicalName; }
        }

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

        public override Destination CreateDestination( String name )
        {
            return new Queue( name );
        }

        public override Byte GetDataStructureType()
        {
            return DataStructureTypes.QueueType;
        }

        public override Int32 GetDestinationType()
        {
            return STOMP_QUEUE;
        }
    }
}