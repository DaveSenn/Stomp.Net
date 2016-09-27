#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public abstract class TempDestination : Destination
    {
        #region Constants

        public const Byte ID_ActiveMQTempDestination = 0;

        #endregion

        #region Properties

        abstract override public DestinationType DestinationType { get; }

        #endregion

        #region Ctor

        protected TempDestination()
        {
        }

        protected TempDestination( String name )
            : base( name )
        {
        }

        #endregion

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (TempDestination) base.Clone();

            // Now do the deep work required
            // If any new variables are added then this routine will
            // likely need updating

            return o;
        }

        /// <summary>
        ///     Method CreateDestination
        /// </summary>
        /// <returns>An Destination</returns>
        /// <param name="name">A  String</param>
        public override Destination CreateDestination( String name ) => null;

        public override Byte GetDataStructureType() => ID_ActiveMQTempDestination;

        /// <summary>
        ///     Method GetDestinationType
        /// </summary>
        /// <returns>An int</returns>
        public override Int32 GetDestinationType() => DataStructureTypes.TempDestinationType;
    }
}