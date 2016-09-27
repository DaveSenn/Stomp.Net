#region Usings

using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.State
{
    public interface ICommandVisitor
    {
        Response ProcessAddConnection( ConnectionInfo info );

        Response ProcessAddConsumer( ConsumerInfo info );

        Response processAddProducer( ProducerInfo info );

        Response processAddSession( SessionInfo info );

        Response processBeginTransaction( TransactionInfo info );

        Response processCommitTransaction( TransactionInfo info );

        Response processConnectionError( ConnectionError error );

        Response processKeepAliveInfo( KeepAliveInfo info );

        Response processMessage( BaseMessage send );

        Response processMessageAck( MessageAck ack );

        Response processMessageDispatch( MessageDispatch dispatch );

        Response ProcessRemoveConnection( ConnectionId id );

        Response processRemoveConsumer( ConsumerId id );

        Response processRemoveProducer( ProducerId id );

        Response processRemoveSession( SessionId id );

        Response processRemoveSubscriptionInfo( RemoveSubscriptionInfo info );

        Response processResponse( Response response );

        Response processRollbackTransaction( TransactionInfo info );

        Response processShutdownInfo( ShutdownInfo info );
    }
}