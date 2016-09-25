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
    public class ProducerId : BaseDataStructure
    {
        #region Fields

        private String key;
        private SessionId parentId;

        #endregion

        #region Properties

        public SessionId ParentId
        {
            get
            {
                if ( parentId == null )
                    parentId = new SessionId( this );
                return parentId;
            }
        }

        public String ConnectionId { get; set; }

        public Int64 Value { get; set; }

        public Int64 SessionId { get; set; }

        #endregion

        #region Ctor

        public ProducerId()
        {
        }

        public ProducerId( SessionId sessionId, Int64 consumerId )
        {
            ConnectionId = sessionId.ConnectionId;
            SessionId = sessionId.Value;
            Value = consumerId;
        }

        public ProducerId( String producerKey )
        {
            // Store the original.
            key = producerKey;

            // Try and get back the AMQ version of the data.
            var idx = producerKey.LastIndexOf( ':' );
            if ( idx >= 0 )
                try
                {
                    Value = Int32.Parse( producerKey.Substring( idx + 1 ) );
                    producerKey = producerKey.Substring( 0, idx );
                    idx = producerKey.LastIndexOf( ':' );
                    if ( idx >= 0 )
                    {
                        SessionId = Int32.Parse( producerKey.Substring( idx + 1 ) );
                        producerKey = producerKey.Substring( 0, idx );
                    }
                }
                catch ( Exception ex )
                {
                    Tracer.Debug( ex.Message );
                }
            ConnectionId = producerKey;
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is ProducerId )
                return Equals( (ProducerId) that );
            return false;
        }

        public virtual Boolean Equals( ProducerId that )
        {
            if ( !Equals( ConnectionId, that.ConnectionId ) )
                return false;
            if ( !Equals( Value, that.Value ) )
                return false;
            if ( !Equals( SessionId, that.SessionId ) )
                return false;

            return true;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
        {
            return DataStructureTypes.ProducerIdType;
        }

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( ConnectionId );
            answer = answer * 37 + HashCode( Value );
            answer = answer * 37 + HashCode( SessionId );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
        {
            if ( key == null )
                key = ConnectionId + ":" + SessionId + ":" + Value;

            return key;
        }
    }
}