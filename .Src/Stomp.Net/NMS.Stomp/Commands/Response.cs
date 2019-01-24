#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class Response : BaseCommand
    {
        public override Byte GetDataStructureType()
            => DataStructureTypes.ResponseType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "CorrelationId=" + CorrelationId +
               "]";

        #region Properties

        public Int32 CorrelationId { get; set; }

        /// <summery>
        ///     Return an answer of true to the isResponse() query.
        /// </summery>
        public override Boolean IsResponse => true;

        #endregion
    }
}