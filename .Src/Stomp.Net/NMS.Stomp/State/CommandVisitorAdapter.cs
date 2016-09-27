#region Usings

using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.State
{
    public class CommandVisitorAdapter : ICommandVisitor
    {
        public virtual Response ProcessAddConnection( ConnectionInfo info ) => null;

        public virtual Response ProcessAddConsumer( ConsumerInfo info ) => null;

        public virtual Response ProcessAddProducer( ProducerInfo info ) => null;

        public virtual Response ProcessAddSession( SessionInfo info ) => null;

        public virtual Response ProcessBeginTransaction( TransactionInfo info ) => null;

        public virtual Response ProcessCommitTransaction( TransactionInfo info ) => null;

        public virtual Response ProcessConnectionError( ConnectionError error ) => null;

        public virtual Response ProcessKeepAliveInfo( KeepAliveInfo info ) => null;

        public virtual Response ProcessMessage( BaseMessage send ) => null;

        public virtual Response ProcessMessageAck( MessageAck ack ) => null;

        public virtual Response ProcessMessageDispatch( MessageDispatch dispatch ) => null;

        public virtual Response ProcessRemoveConnection( ConnectionId id ) => null;

        public virtual Response ProcessRemoveConsumer( ConsumerId id ) => null;

        public virtual Response ProcessRemoveProducer( ProducerId id ) => null;

        public virtual Response ProcessRemoveSession( SessionId id ) => null;

        public virtual Response ProcessRemoveSubscriptionInfo( RemoveSubscriptionInfo info ) => null;

        public virtual Response ProcessResponse( Response response ) => null;

        public virtual Response ProcessRollbackTransaction( TransactionInfo info ) => null;

        public virtual Response ProcessShutdownInfo( ShutdownInfo info ) => null;
    }
}