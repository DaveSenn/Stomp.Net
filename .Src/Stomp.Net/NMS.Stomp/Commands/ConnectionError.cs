#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ConnectionError : BaseCommand
    {
        #region Properties

        public BrokerError Exception { get; set; }

        public ConnectionId ConnectionId { get; set; }

        /// <summery>
        ///     Return an answer of true to the isConnectionError() query.
        /// </summery>
        public override Boolean IsConnectionError
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.ErrorType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "Exception=" + Exception +
                                             "ConnectionId=" + ConnectionId +
                                             "]";

        public override Response Visit( ICommandVisitor visitor ) => visitor.ProcessConnectionError( this );
    }
}