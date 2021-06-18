#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class ShutdownInfo : BaseCommand
    {
        #region Properties

        /// <summery>
        ///     Return an answer of true to the isShutdownInfo() query.
        /// </summery>
        public override Boolean IsShutdownInfo => true;

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.ShutdownInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                .Name + "[" + "]";
    }
}