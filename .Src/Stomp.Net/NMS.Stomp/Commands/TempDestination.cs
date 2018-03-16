#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public abstract class TempDestination : Destination
    {
        #region Constants

        private const Byte IdActiveMqTempDestination = 0;

        #endregion

        #region Properties

        public abstract override DestinationType DestinationType { get; }

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TempDestination" /> class with the given physical name.
        /// </summary>
        /// <param name="name">The physical name of the destination.</param>
        /// <param name="skipDesinationNameFormatting">
        ///     A value indicating whether the destination name formatting will be skipped
        ///     or not.
        /// </param>
        public TempDestination( String name, Boolean skipDesinationNameFormatting )
            : base( name, skipDesinationNameFormatting )
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
        [PublicAPI]
        public virtual Destination CreateDestination( String name )
            => null;

        public override Byte GetDataStructureType()
            => IdActiveMqTempDestination;

        /// <summary>
        ///     Method GetDestinationType
        /// </summary>
        /// <returns>An int</returns>
        protected override Int32 GetDestinationType()
            => DataStructureTypes.TempDestinationType;
    }
}