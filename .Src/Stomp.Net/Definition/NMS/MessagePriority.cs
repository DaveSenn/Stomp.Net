#region Usings

using JetBrains.Annotations;

#endregion

namespace Stomp.Net;

/// <summary>
///     Define an enumerated array of message priorities.
/// </summary>
public enum MessagePriority
{
    [PublicAPI]
    Lowest = 0,

    [PublicAPI]
    VeryLow = 1,

    [PublicAPI]
    Low = 2,

    [PublicAPI]
    AboveLow = 3,

    [PublicAPI]
    BelowNormal = 4,

    [PublicAPI]
    Normal = 5,

    [PublicAPI]
    AboveNormal = 6,

    [PublicAPI]
    High = 7,

    [PublicAPI]
    VeryHigh = 8,

    [PublicAPI]
    Highest = 9
}