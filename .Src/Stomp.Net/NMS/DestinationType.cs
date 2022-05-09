namespace Stomp.Net;

/// <summary>
///     Represents the type of the destination such as a queue or topic.
/// </summary>
public enum DestinationType
{
    Queue,
    Topic,
    TemporaryQueue,
    TemporaryTopic
}