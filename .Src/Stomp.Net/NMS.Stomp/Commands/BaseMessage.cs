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
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class BaseMessage : BaseCommand, MarshallAware, ICloneable
    {
        #region Constants

        public const Int32 DEFAULT_MINIMUM_MESSAGE_SIZE = 1024;

        #endregion

        #region Properties

        public ProducerId ProducerId { get; set; }

        public Destination Destination { get; set; }

        public TransactionId TransactionId { get; set; }

        public MessageId MessageId { get; set; }

        public TransactionId OriginalTransactionId { get; set; }

        public String GroupID { get; set; }

        public Int32 GroupSequence { get; set; }

        public String CorrelationId { get; set; }

        public Boolean Persistent { get; set; }

        public Int64 Expiration { get; set; }

        public Byte Priority { get; set; }

        public Destination ReplyTo { get; set; }

        public Int64 Timestamp { get; set; }

        public String Type { get; set; }

        public Byte[] Content { get; set; }

        public Byte[] MarshalledProperties { get; set; }

        public ConsumerId TargetConsumerId { get; set; }

        public Int32 RedeliveryCounter { get; set; }

        public virtual Boolean ReadOnlyProperties { get; set; }

        public virtual Boolean ReadOnlyBody { get; set; }

        /// <summery>
        ///     Return an answer of true to the isMessage() query.
        /// </summery>
        public override Boolean IsMessage
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Clone this object and return a new instance that the caller now owns.
        /// </summery>
        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (Message) base.Clone();

            if ( MessageId != null )
                o.MessageId = (MessageId) MessageId.Clone();

            return o;
        }

        public Boolean IsExpired() => Expiration == 0 ? false : DateTime.UtcNow > DateUtils.ToDateTimeUtc( Expiration );

        public virtual void OnMessageRollback()
        {
            RedeliveryCounter++;
        }

        public virtual void OnSend()
        {
            ReadOnlyProperties = true;
            ReadOnlyBody = true;
        }

        public virtual Int32 Size()
        {
            var size = DEFAULT_MINIMUM_MESSAGE_SIZE;

            if ( MarshalledProperties != null )
                size += MarshalledProperties.Length;
            if ( Content != null )
                size += Content.Length;

            return size;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "ProducerId=" + ProducerId + ", " +
                                             "Destination=" + Destination + ", " +
                                             "TransactionId=" + TransactionId + ", " +
                                             "MessageId=" + MessageId + ", " +
                                             "OriginalTransactionId=" + OriginalTransactionId + ", " +
                                             "GroupID=" + GroupID + ", " +
                                             "GroupSequence=" + GroupSequence + ", " +
                                             "CorrelationId=" + CorrelationId + ", " +
                                             "Persistent=" + Persistent + ", " +
                                             "Expiration=" + Expiration + ", " +
                                             "Priority=" + Priority + ", " +
                                             "ReplyTo=" + ReplyTo + ", " +
                                             "Timestamp=" + Timestamp + ", " +
                                             "Type=" + Type + ", " +
                                             "Content=" + Content + ", " +
                                             "MarshalledProperties=" + MarshalledProperties + ", " +
                                             "TargetConsumerId=" + TargetConsumerId + ", " +
                                             "RedeliveryCounter=" + RedeliveryCounter +
                                             "]";

        public override Response visit( ICommandVisitor visitor ) => visitor.processMessage( this );
    }
}