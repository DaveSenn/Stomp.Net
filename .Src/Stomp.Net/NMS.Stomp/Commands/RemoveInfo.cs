#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class RemoveInfo : BaseCommand
    {
        #region Properties

        public IDataStructure ObjectId { get; set; }

        /// <summery>
        ///     Return an answer of true to the isRemoveInfo() query.
        /// </summery>
        public override Boolean IsRemoveInfo => true;

        #endregion

        public override Byte GetDataStructureType()
            => DataStructureTypes.RemoveInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => GetType()
                   .Name + "[" +
               "ObjectId=" + ObjectId +
               "]";

        /// <summery>
        ///     Allows a Visitor to Visit this command and return a response to the
        ///     command based on the command type being visited.  The command will call
        ///     the proper processXXX method in the visitor.
        /// </summery>
        public override Response Visit( ICommandVisitor visitor )
        {
            switch ( ObjectId.GetDataStructureType() )
            {
                case DataStructureTypes.ConnectionIdType:
                    return visitor.ProcessRemoveConnection( (ConnectionId) ObjectId );
                case DataStructureTypes.SessionIdType:
                    return visitor.ProcessRemoveSession( (SessionId) ObjectId );
                case DataStructureTypes.ConsumerIdType:
                    return visitor.ProcessRemoveConsumer( (ConsumerId) ObjectId );
                case DataStructureTypes.ProducerIdType:
                    return visitor.ProcessRemoveProducer( (ProducerId) ObjectId );
                default:
                    throw new IoException( "Unknown remove command type: " + ObjectId.GetDataStructureType() );
            }
        }
    }
}