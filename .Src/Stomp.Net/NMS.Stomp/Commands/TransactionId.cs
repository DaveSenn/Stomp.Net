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
    public class TransactionId : BaseDataStructure
    {
        #region Properties

        public Int64 Value { get; set; }

        public ConnectionId ConnectionId { get; set; }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is TransactionId )
                return Equals( (TransactionId) that );
            return false;
        }

        public virtual Boolean Equals( TransactionId that )
        {
            if ( !Equals( Value, that.Value ) )
                return false;
            if ( !Equals( ConnectionId, that.ConnectionId ) )
                return false;

            return true;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.TransactionIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( Value );
            answer = answer * 37 + HashCode( ConnectionId );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => ConnectionId + ":" + Value;
    }
}