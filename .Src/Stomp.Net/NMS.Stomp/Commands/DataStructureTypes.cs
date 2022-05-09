#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands;

public class DataStructureTypes
{
    public static String GetDataStructureTypeAsString( Int32 type )
    {
        var packetTypeStr = type switch
        {
            ErrorType => "ErrorType",
            MessageType => "MessageType",
            BytesMessageType => "BytesMessageType",
            StreamMessageType => "StreamMessageType",
            TextMessageType => "TextMessageType",
            MessageDispatchType => "MessageDispatchType",
            MessageIdType => "MessageIdType",
            MessageAckType => "MessageAckType",
            ConnectionInfoType => "ConnectionInfoType",
            ConnectionIdType => "ConnectionIdType",
            ConsumerInfoType => "ConsumerInfoType",
            ConsumerIdType => "ConsumerIdType",
            ProducerInfoType => "ProducerInfoType",
            ProducerIdType => "ProducerIdType",
            SessionInfoType => "SessionInfoType",
            TransactionInfoType => "TransactionInfoType",
            TransactionIdType => "TransactionIdType",
            SubscriptionInfoType => "SubscriptionInfoType",
            ShutdownInfoType => "ShutdownInfoType",
            ResponseType => "ResponseType",
            RemoveInfoType => "RemoveInfoType",
            ErrorResponseType => "ErrorResponseType",
            KeepAliveInfoType => "KeepAliveInfoType",
            WireFormatInfoType => "WireFormatInfoType",
            DestinationType => "DestinationType",
            TempDestinationType => "TempDestinationType",
            TopicType => "TopicType",
            TempTopicType => "TempTopicType",
            QueueType => "QueueType",
            TempQueueType => "TempQueueType",
            _ => "UnknownType"
        };

        return packetTypeStr;
    }

    #region Constants

    public const Byte BytesMessageType = 2;
    public const Byte ConnectionIdType = 13;

    public const Byte ConnectionInfoType = 12;
    public const Byte ConsumerIdType = 15;
    private const Byte ConsumerInfoType = 14;

    private const Byte DestinationType = 48;
    public const Byte ErrorResponseType = 27;
    public const Byte ErrorType = 0;
    public const Byte KeepAliveInfoType = 28;
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
    private const Byte WireFormatInfoType = 29;

    #endregion
}