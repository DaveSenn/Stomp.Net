#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class SessionInfo : BaseCommand
    {
        #region Properties

        public SessionId SessionId { get; set; }

        #endregion

        #region Ctor

        public SessionInfo()
        {
        }

        public SessionInfo( ConnectionInfo connectionInfo, Int64 sessionId )
        {
            SessionId = new SessionId( connectionInfo.ConnectionId, sessionId );
        }

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

        public override Response Visit( ICommandVisitor visitor )
            => visitor.ProcessAddSession( this );
    }
}