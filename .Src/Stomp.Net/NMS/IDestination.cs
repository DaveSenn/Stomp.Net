#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     A base interface for destinations such as queues or topics
    /// </summary>
    public interface IDestination
    {
        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether the destination name formatting should be skipped or not.
        ///     If set to true the physical name property will be used as stomp destination string without adding prefixes such as
        ///     queue or topic. This to support JMS brokers listening for queue/topic names in a different format.
        /// </summary>
        Boolean SkipDesinationNameFormatting { get; }

        DestinationType DestinationType { get; }

        Boolean IsTopic { get; }

        Boolean IsQueue { get; }

        Boolean IsTemporary { get; }

        #endregion
    }
}