#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ProducerInfo : BaseCommand
    {
        #region Properties

        public ProducerId ProducerId { get; set; }

        public Destination Destination { get; set; }

        public Boolean DispatchAsync { get; set; }

        /// <summery>
        ///     Return an answer of true to the isProducerInfo() query.
        /// </summery>
        public override Boolean IsProducerInfo => true;

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.ProducerInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "ProducerId=" + ProducerId + ", " +
                                             "Destination=" + Destination + ", " +
                                             "DispatchAsync=" + DispatchAsync +
                                             "]";

        public override Response Visit( ICommandVisitor visitor )
            => visitor.ProcessAddProducer( this );
    }
}