/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.State
{
    public interface ICommandVisitor
    {
        Response processAddConnection( ConnectionInfo info );

        Response processAddConsumer( ConsumerInfo info );

        Response processAddProducer( ProducerInfo info );

        Response processAddSession( SessionInfo info );

        Response processBeginTransaction( TransactionInfo info );

        Response processCommitTransaction( TransactionInfo info );

        Response processConnectionError( ConnectionError error );

        Response processKeepAliveInfo( KeepAliveInfo info );

        Response processMessage( BaseMessage send );

        Response processMessageAck( MessageAck ack );

        Response processMessageDispatch( MessageDispatch dispatch );

        Response processRemoveConnection( ConnectionId id );

        Response processRemoveConsumer( ConsumerId id );

        Response processRemoveProducer( ProducerId id );

        Response processRemoveSession( SessionId id );

        Response processRemoveSubscriptionInfo( RemoveSubscriptionInfo info );

        Response processResponse( Response response );

        Response processRollbackTransaction( TransactionInfo info );

        Response processShutdownInfo( ShutdownInfo info );
    }
}