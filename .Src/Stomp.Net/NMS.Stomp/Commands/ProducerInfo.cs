#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class ProducerInfo : BaseCommand
    {
        #region Properties

        public ProducerId ProducerId { get; set; }

        public Destination Destination { get; set; }

        public Boolean DispatchAsync { get; set; }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.ProducerInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "ProducerId=" + ProducerId + ", " +
               "Destination=" + Destination + ", " +
               "DispatchAsync=" + DispatchAsync +
               "]";
    }
}