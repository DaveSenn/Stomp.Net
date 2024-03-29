#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net;

/// <summary>
///     Represents a queue in a message broker. A message sent to a queue is delivered
///     to at most one consumer on the queue.
/// </summary>
public interface IQueue : IDestination
{
    #region Properties

    [PublicAPI]
    String QueueName { get; }

    #endregion
}