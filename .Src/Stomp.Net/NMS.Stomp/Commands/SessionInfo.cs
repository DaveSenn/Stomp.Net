#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class SessionInfo : BaseCommand
    {
        #region Properties

        public SessionId SessionId { get; set; }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.SessionInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" + "SessionId=" + SessionId + "]";
    }
}