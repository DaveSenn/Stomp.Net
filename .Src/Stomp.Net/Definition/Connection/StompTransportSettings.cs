#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Class representing the STOMp transport settings.
    /// </summary>
    public class StompTransportSettings
    {
        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether logging is enabled or not.
        /// </summary>
        /// <value>A value indicating whether logging is enabled or not.</value>
        public Boolean UseLogging { get; set; }

        /// <summary>
        ///     gets or sets a value indicating whether the inactivity monitor is enabled or not.
        /// </summary>
        /// <remarks>
        ///     Default is true.
        /// </remarks>
        /// <value>A value indicating whether the inactivity monitor is enabled or not.</value>
        public Boolean UseInactivityMonitor { get; set; } = true;

        /// <summary>
        ///     Gets or sets the size of the buffer used to receive data.
        /// </summary>
        /// <remarks>
        ///     Default is 8192.
        /// </remarks>
        /// <value>The size of the buffer used to receive data.</value>
        public Int32 ReceiveBufferSize { get; set; } = 8192;

        /// <summary>
        ///     Gets or sets the size of the buffer used to send data.
        /// </summary>
        /// <remarks>
        ///     Default is 8192.
        /// </remarks>
        /// <value>The size of the buffer used to send data.</value>
        public Int32 SendBufferSize { get; set; } = 8192;

        /// <summary>
        ///     Gets or sets the receive timeout.
        /// </summary>
        /// <remarks>
        ///     The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period.
        ///     Specifying -1 also indicates an infinite time-out period.
        /// </remarks>
        /// <value>The receive timeout.</value>
        public Int32 ReceiveTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the send timeout.
        /// </summary>
        /// <remarks>
        ///     The time-out value, in milliseconds. If you set the property with a value between 1 and 499, the value will be
        ///     changed to 500. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates
        ///     an infinite time-out period.
        /// </remarks>
        /// <value>The send timeout.</value>
        public Int32 SendTimeout { get; set; }

        /// <summary>
        ///     Gets the SSL settings.
        /// </summary>
        /// <value>The SSL settings.</value>
        public StompSslSettings SslSettings { get; } = new();

        #endregion
    }
}