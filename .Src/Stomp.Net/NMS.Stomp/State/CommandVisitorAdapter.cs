#region Usings

using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.State
{
    public class CommandVisitorAdapter : ICommandVisitor
    {
        public virtual Response ProcessAddConnection( ConnectionInfo info ) => null;

        public virtual Response ProcessAddConsumer( ConsumerInfo info ) => null;

        public virtual Response processAddProducer( ProducerInfo info ) => null;

        public virtual Response processAddSession( SessionInfo info ) => null;

        public virtual Response processBeginTransaction( TransactionInfo info ) => null;

        public virtual Response processCommitTransaction( TransactionInfo info ) => null;

        public virtual Response processConnectionError( ConnectionError error ) => null;

        public virtual Response processKeepAliveInfo( KeepAliveInfo info ) => null;

        public virtual Response processMessage( BaseMessage send ) => null;

        public virtual Response processMessageAck( MessageAck ack ) => null;

        public virtual Response processMessageDispatch( MessageDispatch dispatch ) => null;

        public virtual Response ProcessRemoveConnection( ConnectionId id ) => null;

        public virtual Response processRemoveConsumer( ConsumerId id ) => null;

        public virtual Response processRemoveProducer( ProducerId id ) => null;

        public virtual Response processRemoveSession( SessionId id ) => null;

        public virtual Response processRemoveSubscriptionInfo( RemoveSubscriptionInfo info ) => null;

        public virtual Response processResponse( Response response ) => null;

        public virtual Response processRollbackTransaction( TransactionInfo info ) => null;

        public virtual Response processShutdownInfo( ShutdownInfo info ) => null;
    }
}