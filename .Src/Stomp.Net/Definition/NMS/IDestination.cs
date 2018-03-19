#region Usings

using System;
using JetBrains.Annotations;

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
        [PublicAPI]
        Boolean SkipDesinationNameFormatting { get; }

        [PublicAPI]
        DestinationType DestinationType { get; }

        [PublicAPI]
        Boolean IsTopic { get; }

        [PublicAPI]
        Boolean IsQueue { get; }

        [PublicAPI]
        Boolean IsTemporary { get; }

        /// <summary>
        ///     Gets the name of the destination.
        /// </summary>
        /// <value>The name of the destination.</value>
        [PublicAPI]
        String PhysicalName { get; }

        #endregion
    }
}