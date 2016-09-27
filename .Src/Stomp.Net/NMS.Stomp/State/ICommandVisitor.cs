#region Usings

using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.State
{
    public interface ICommandVisitor
    {
        Response ProcessAddConnection( ConnectionInfo info );

        Response ProcessAddConsumer( ConsumerInfo info );

        Response ProcessAddProducer( ProducerInfo info );

        Response ProcessAddSession( SessionInfo info );

        Response ProcessBeginTransaction( TransactionInfo info );

        Response ProcessCommitTransaction( TransactionInfo info );

        Response ProcessConnectionError( ConnectionError error );

        Response ProcessKeepAliveInfo( KeepAliveInfo info );

        Response ProcessMessage( BaseMessage send );

        Response ProcessMessageAck( MessageAck ack );

        Response ProcessMessageDispatch( MessageDispatch dispatch );

        Response ProcessRemoveConnection( ConnectionId id );

        Response ProcessRemoveConsumer( ConsumerId id );

        Response ProcessRemoveProducer( ProducerId id );

        Response ProcessRemoveSession( SessionId id );

        Response ProcessRemoveSubscriptionInfo( RemoveSubscriptionInfo info );

        Response ProcessResponse( Response response );

        Response ProcessRollbackTransaction( TransactionInfo info );

        Response ProcessShutdownInfo( ShutdownInfo info );
    }
}