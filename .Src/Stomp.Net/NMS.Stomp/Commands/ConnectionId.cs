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

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ConnectionId : BaseDataStructure
    {
        #region Properties

        public String Value { get; set; }

        #endregion

        #region Ctor

        public ConnectionId()
        {
        }

        public ConnectionId( SessionId sessionId )
        {
            Value = sessionId.ConnectionId;
        }

        public ConnectionId( ProducerId producerId )
        {
            Value = producerId.ConnectionId;
        }

        public ConnectionId( ConsumerId consumerId )
        {
            Value = consumerId.ConnectionId;
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is ConnectionId )
                return Equals( (ConnectionId) that );
            return false;
        }

        public virtual Boolean Equals( ConnectionId that ) => Equals( Value, that.Value );

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.ConnectionIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( Value );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => Value;
    }
}