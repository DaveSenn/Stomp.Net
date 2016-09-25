

#region Usings

using System;

#endregion

namespace Apache.NMS
{
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

    /// <summary>
    ///     A base interface for destinations such as queues or topics
    /// </summary>
    public interface IDestination : IDisposable
    {
        #region Properties

        DestinationType DestinationType { get; }

        Boolean IsTopic { get; }
        Boolean IsQueue { get; }
        Boolean IsTemporary { get; }

        #endregion
    }
}