

using System;

namespace Apache.NMS
{
    /// <summary>
    ///     Represents a topic in a message broker. A message sent to a topic
    ///     is delivered to all consumers on the topic who are interested in the message.
    /// </summary>
    public interface ITopic : IDestination
    {
        #region Properties

        String TopicName { get; }

        #endregion
    }
}