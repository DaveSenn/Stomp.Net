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
    public class ConsumerInfo : BaseCommand
    {
        #region Constants

        public const Byte ID_CONSUMERINFO = 5;

        #endregion

        #region Fields

        #endregion

        #region Properties

        public ConsumerId ConsumerId { get; set; }

        public Destination Destination { get; set; }

        public AcknowledgementMode AckMode { get; set; }

        public Int32 PrefetchSize { get; set; }

        public Int32 MaximumPendingMessageLimit { get; set; }

        public Boolean DispatchAsync { get; set; }

        public String Selector { get; set; }

        public String SubscriptionName { get; set; }

        public Boolean NoLocal { get; set; }

        public Boolean Exclusive { get; set; }

        public Boolean Retroactive { get; set; }

        public Byte Priority { get; set; }

        public String Transformation { get; set; }

        /// <summery>
        ///     Return an answer of true to the isConsumerInfo() query.
        /// </summery>
        public override Boolean IsConsumerInfo
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => ID_CONSUMERINFO;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" +
                                             "ConsumerId=" + ConsumerId + ", " +
                                             "Destination=" + Destination + ", " +
                                             "Ack Mode=" + AckMode + ", " +
                                             "PrefetchSize=" + PrefetchSize + ", " +
                                             "MaximumPendingMessageLimit=" + MaximumPendingMessageLimit + ", " +
                                             "DispatchAsync=" + DispatchAsync + ", " +
                                             "Selector=" + Selector + ", " +
                                             "SubscriptionName=" + SubscriptionName + ", " +
                                             "NoLocal=" + NoLocal + ", " +
                                             "Exclusive=" + Exclusive + ", " +
                                             "Retroactive=" + Retroactive + ", " +
                                             "Priority=" + Priority + ", " +
                                             "Transformation" + Transformation +
                                             "]";

        public override Response visit( ICommandVisitor visitor ) => visitor.processAddConsumer( this );
    }
}