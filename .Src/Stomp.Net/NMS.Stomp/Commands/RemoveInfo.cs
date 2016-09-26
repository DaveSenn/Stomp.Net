/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class RemoveInfo : BaseCommand
    {
        #region Fields

        #endregion

        #region Properties

        public DataStructure ObjectId { get; set; }

        /// <summery>
        ///     Return an answer of true to the isRemoveInfo() query.
        /// </summery>
        public override Boolean IsRemoveInfo
        {
            get { return true; }
        }

        #endregion

        public override Byte GetDataStructureType() => DataStructureTypes.RemoveInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "ObjectId=" + ObjectId +
                                             "]";

        /// <summery>
        ///     Allows a Visitor to visit this command and return a response to the
        ///     command based on the command type being visited.  The command will call
        ///     the proper processXXX method in the visitor.
        /// </summery>
        public override Response visit( ICommandVisitor visitor )
        {
            switch ( ObjectId.GetDataStructureType() )
            {
                case DataStructureTypes.ConnectionIdType:
                    return visitor.processRemoveConnection( (ConnectionId) ObjectId );
                case DataStructureTypes.SessionIdType:
                    return visitor.processRemoveSession( (SessionId) ObjectId );
                case DataStructureTypes.ConsumerIdType:
                    return visitor.processRemoveConsumer( (ConsumerId) ObjectId );
                case DataStructureTypes.ProducerIdType:
                    return visitor.processRemoveProducer( (ProducerId) ObjectId );
                default:
                    throw new IOException( "Unknown remove command type: " + ObjectId.GetDataStructureType() );
            }
        }
    }
}