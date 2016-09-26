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
    public class MessageDispatch : BaseCommand
    {
        #region Properties

        public ConsumerId ConsumerId { get; set; }

        public Destination Destination { get; set; }

        public Message Message { get; set; }

        public Int32 RedeliveryCounter { get; set; }

        /// <summery>
        ///     Return an answer of true to the isMessageDispatch() query.
        /// </summery>
        public override Boolean IsMessageDispatch
        {
            get { return true; }
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is MessageDispatch )
                return Equals( (MessageDispatch) that );
            return false;
        }

        public virtual Boolean Equals( MessageDispatch that )
        {
            if ( !Equals( ConsumerId, that.ConsumerId ) )
                return false;
            if ( !Equals( Destination, that.Destination ) )
                return false;
            if ( !Equals( Message, that.Message ) )
                return false;
            if ( !Equals( RedeliveryCounter, that.RedeliveryCounter ) )
                return false;

            return true;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.MessageDispatchType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( ConsumerId );
            answer = answer * 37 + HashCode( Destination );
            answer = answer * 37 + HashCode( Message );
            answer = answer * 37 + HashCode( RedeliveryCounter );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "ConsumerId=" + ConsumerId + ", " +
                                             "Destination=" + Destination + ", " +
                                             "Message=" + Message + ", " +
                                             "RedeliveryCounter=" + RedeliveryCounter +
                                             "]";

        public override Response visit( ICommandVisitor visitor ) => visitor.processMessageDispatch( this );
    }
}