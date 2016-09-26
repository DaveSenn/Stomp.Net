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
    public class MessageId : BaseDataStructure
    {
        #region Fields

        private String key;
        private Int64 producerSequenceId;

        #endregion

        #region Properties

        public ProducerId ProducerId { get; set; }

        public Int64 ProducerSequenceId
        {
            get { return producerSequenceId; }
            set { producerSequenceId = value; }
        }

        public Int64 BrokerSequenceId { get; set; }

        #endregion

        #region Ctor

        public MessageId()
        {
        }

        public MessageId( ProducerId prodId, Int64 producerSeqId )
        {
            ProducerId = prodId;
            producerSequenceId = producerSeqId;
        }

        public MessageId( String value )
        {
            SetValue( value );
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is MessageId )
                return Equals( (MessageId) that );

            return false;
        }

        public virtual Boolean Equals( MessageId that ) => Equals( ProducerId, that.ProducerId )
                                                           && Equals( ProducerSequenceId, that.ProducerSequenceId )
                                                           && Equals( BrokerSequenceId, that.BrokerSequenceId );

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.MessageIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( ProducerId );
            answer = answer * 37 + HashCode( ProducerSequenceId );
            answer = answer * 37 + HashCode( BrokerSequenceId );

            return answer;
        }

        /// <summary>
        ///     Sets the value as a String
        /// </summary>
        public void SetValue( String messageKey )
        {
            var mkey = messageKey;

            key = mkey;

            // Parse off the sequenceId
            var p = mkey.LastIndexOf( ":" );
            if ( p >= 0 )
                if ( Int64.TryParse( mkey.Substring( p + 1 ), out producerSequenceId ) )
                    mkey = mkey.Substring( 0, p );
                else
                    producerSequenceId = 0;

            ProducerId = new ProducerId( mkey );
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
        {
            if ( null == key )
                key = String.Format( "{0}:{1}", ProducerId, producerSequenceId );

            return key;
        }
    }
}