#region Usings

using System;
using Apache.NMS;
using Apache.NMS.Stomp;
using Apache.NMS.Stomp.Util;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     STOMP connection settings.
    /// </summary>
    public class StompConnectionSettings
    {
        #region Properties

        /// <summary>
        ///     Gets or sets a client id generator.
        /// </summary>
        public IdGenerator ClientIdGenerator { get; set; }

        /// <summary>
        ///     Gets or sets a client id prefix used together with <see cref="ClientIdGenerator" />.
        /// </summary>
        public String ClientIdPrefix { get; set; }

        /// <summary>
        ///     Gets or sets the client id.
        /// </summary>
        public String ClientId { get; set; }

        public String UserName { get; set; }
        public String Password { get; set; }
        public Boolean AsyncSend { get; set; }
        public Boolean CopyMessageOnSend { get; set; } = true;
        public Boolean AlwaysSyncSend { get; set; }
        public Boolean SendAcksAsync { get; set; } = true;

        public Boolean DispatchAsync { get; set; } = true;

        /// <summary>
        ///     Gets or sets a <see cref="AcknowledgementMode" />.
        /// </summary>
        public AcknowledgementMode AcknowledgementMode { get; set; } = StompNetConfiguration.DefaultAcknowledgementMode;

        /// <summary>
        ///     Gets or sets the request timeout.
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = StompNetConfiguration.RequestTimeout;

        /// <summary>
        ///     Gets a <see cref="PrefetchPolicy" />.
        /// </summary>
        public PrefetchPolicy PrefetchPolicy { get; } = new PrefetchPolicy();

        /// <summary>
        ///     Gets the transport settings.
        /// </summary>
        /// <value>The transport settings.</value>
        public StompTransportSettings TransportSettings { get; } = new StompTransportSettings();

        #endregion
    }
}