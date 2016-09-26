#region Usings

using System;
using System.Threading;
using Apache.NMS;

#endregion

namespace Stomp.Net
{
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

        #endregion
    }
}