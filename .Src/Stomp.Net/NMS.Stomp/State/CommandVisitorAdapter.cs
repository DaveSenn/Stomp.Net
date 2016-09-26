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
    public class CommandVisitorAdapter : ICommandVisitor
    {
        public virtual Response processAddConnection( ConnectionInfo info ) => null;

        public virtual Response processAddConsumer( ConsumerInfo info ) => null;

        public virtual Response processAddProducer( ProducerInfo info ) => null;

        public virtual Response processAddSession( SessionInfo info ) => null;

        public virtual Response processBeginTransaction( TransactionInfo info ) => null;

        public virtual Response processCommitTransaction( TransactionInfo info ) => null;

        public virtual Response processConnectionError( ConnectionError error ) => null;

        public virtual Response processKeepAliveInfo( KeepAliveInfo info ) => null;

        public virtual Response processMessage( BaseMessage send ) => null;

        public virtual Response processMessageAck( MessageAck ack ) => null;

        public virtual Response processMessageDispatch( MessageDispatch dispatch ) => null;

        public virtual Response processRemoveConnection( ConnectionId id ) => null;

        public virtual Response processRemoveConsumer( ConsumerId id ) => null;

        public virtual Response processRemoveProducer( ProducerId id ) => null;

        public virtual Response processRemoveSession( SessionId id ) => null;

        public virtual Response processRemoveSubscriptionInfo( RemoveSubscriptionInfo info ) => null;

        public virtual Response processResponse( Response response ) => null;

        public virtual Response processRollbackTransaction( TransactionInfo info ) => null;

        public virtual Response processShutdownInfo( ShutdownInfo info ) => null;
    }
}