#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class DataStructureTypes
    {
        #region Constants

        public const Byte BytesMessageType = 2;
        public const Byte ConnectionIdType = 13;

        public const Byte ConnectionInfoType = 12;
        public const Byte ConsumerIdType = 15;
        public const Byte ConsumerInfoType = 14;

        public const Byte DestinationType = 48;
        public const Byte ErrorResponseType = 27;
        public const Byte ErrorType = 0;
        public const Byte KeepAliveInfoType = 28;
        public const Byte MapMessageType = 3;
        public const Byte MessageAckType = 11;

        public const Byte MessageDispatchType = 9;
        public const Byte MessageIdType = 10;

        public const Byte MessageType = 1;
        public const Byte ProducerIdType = 17;
        public const Byte ProducerInfoType = 16;
        public const Byte QueueType = 52;
        public const Byte RemoveInfoType = 25;
        public const Byte RemoveSubscriptionInfoType = 26;
        public const Byte ResponseType = 24;
        public const Byte SessionIdType = 19;
        public const Byte SessionInfoType = 18;
        public const Byte ShutdownInfoType = 23;
        public const Byte StreamMessageType = 4;
        public const Byte SubscriptionInfoType = 22;
        public const Byte TempDestinationType = 49;
        public const Byte TempQueueType = 53;
        public const Byte TempTopicType = 51;
        public const Byte TextMessageType = 5;
        public const Byte TopicType = 50;
        public const Byte TransactionIdType = 21;
        public const Byte TransactionInfoType = 20;
        public const Byte WireFormatInfoType = 29;

        #endregion

        public static String GetDataStructureTypeAsString( Int32 type )
        {
            var packetTypeStr = "UnknownType";
            switch ( type )
            {
                case ErrorType:
                    packetTypeStr = "ErrorType";
                    break;
                case MessageType:
                    packetTypeStr = "MessageType";
                    break;
                case BytesMessageType:
                    packetTypeStr = "BytesMessageType";
                    break;
                case StreamMessageType:
                    packetTypeStr = "StreamMessageType";
                    break;
                case TextMessageType:
                    packetTypeStr = "TextMessageType";
                    break;
                case MessageDispatchType:
                    packetTypeStr = "MessageDispatchType";
                    break;
                case MessageIdType:
                    packetTypeStr = "MessageIdType";
                    break;
                case MessageAckType:
                    packetTypeStr = "MessageAckType";
                    break;
                case ConnectionInfoType:
                    packetTypeStr = "ConnectionInfoType";
                    break;
                case ConnectionIdType:
                    packetTypeStr = "ConnectionIdType";
                    break;
                case ConsumerInfoType:
                    packetTypeStr = "ConsumerInfoType";
                    break;
                case ConsumerIdType:
                    packetTypeStr = "ConsumerIdType";
                    break;
                case ProducerInfoType:
                    packetTypeStr = "ProducerInfoType";
                    break;
                case ProducerIdType:
                    packetTypeStr = "ProducerIdType";
                    break;
                case SessionInfoType:
                    packetTypeStr = "SessionInfoType";
                    break;
                case TransactionInfoType:
                    packetTypeStr = "TransactionInfoType";
                    break;
                case TransactionIdType:
                    packetTypeStr = "TransactionIdType";
                    break;
                case SubscriptionInfoType:
                    packetTypeStr = "SubscriptionInfoType";
                    break;
                case ShutdownInfoType:
                    packetTypeStr = "ShutdownInfoType";
                    break;
                case ResponseType:
                    packetTypeStr = "ResponseType";
                    break;
                case RemoveInfoType:
                    packetTypeStr = "RemoveInfoType";
                    break;
                case ErrorResponseType:
                    packetTypeStr = "ErrorResponseType";
                    break;
                case KeepAliveInfoType:
                    packetTypeStr = "KeepAliveInfoType";
                    break;
                case WireFormatInfoType:
                    packetTypeStr = "WireFormatInfoType";
                    break;
                case DestinationType:
                    packetTypeStr = "DestinationType";
                    break;
                case TempDestinationType:
                    packetTypeStr = "TempDestinationType";
                    break;
                case TopicType:
                    packetTypeStr = "TopicType";
                    break;
                case TempTopicType:
                    packetTypeStr = "TempTopicType";
                    break;
                case QueueType:
                    packetTypeStr = "QueueType";
                    break;
                case TempQueueType:
                    packetTypeStr = "TempQueueType";
                    break;
            }

            return packetTypeStr;
        }
    }
}