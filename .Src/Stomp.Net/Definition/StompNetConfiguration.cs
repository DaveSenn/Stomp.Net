#region Usings

using System;
using System.Threading;

#endregion

namespace Stomp.Net;

/// <summary>
///     Static configuration for Stomp.Net.
/// </summary>
public static class StompNetConfiguration
{
    #region Properties

    /// <summary>
    ///     Gets or sets the default acknowledge mode.
    /// </summary>
    /// <value>The default acknowledge mode.</value>
    public static AcknowledgementMode DefaultAcknowledgementMode { get; set; } = AcknowledgementMode.AutoAcknowledge;

    /// <summary>
    ///     Gets or sets the default connection timeout.
    /// </summary>
    /// <value>The default connection timeout.</value>
    public static TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMilliseconds( Timeout.Infinite );

    /// <summary>
    ///     Gets or sets a value indicating whether a content-length header will be added to messages during sending or not.
    /// </summary>
    public static Boolean AddContentLengthHeader { get; set; } = true;

    #endregion
}