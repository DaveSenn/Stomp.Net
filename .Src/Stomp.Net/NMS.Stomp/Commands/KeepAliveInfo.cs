#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class KeepAliveInfo : BaseCommand
    {
        #region Properties

        /// <summery>
        ///     Return an answer of true to the isKeepAliveInfo() query.
        /// </summery>
        public override Boolean IsKeepAliveInfo => true;

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.KeepAliveInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[ " +
               "commandId = " + CommandId + ", " +
               "responseRequired = " + ResponseRequired + ", " + " ]";
    }
}