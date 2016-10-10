#region Usings

using System;
using Stomp.Net.Stomp;
using Stomp.Net.Stomp.Util;

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

        /// <summary>
        ///     Gets or set the user name.
        /// </summary>
        /// <value>The user name.</value>
        public String UserName { get; set; }

        /// <summary>
        ///     Gets or set the password.
        /// </summary>
        /// <value>The password.</value>
        public String Password { get; set; }

        /// <summary>
        ///     Gets or sets the async send option.
        /// </summary>
        /// <value>The async send option.</value>
        public Boolean AsyncSend { get; set; }

        /// <summary>
        ///     Gets or sets the copy message on send option.
        /// </summary>
        /// <value>The copy message on send option.</value>
        public Boolean CopyMessageOnSend { get; set; } = true;

        /// <summary>
        ///     Gets or sets the always sync send option.
        /// </summary>
        /// <value>The copy always sync send option.</value>
        public Boolean AlwaysSyncSend { get; set; }

        /// <summary>
        ///     Gets or sets the send ACK async option.
        /// </summary>
        /// <value>The send ACK async option.</value>
        public Boolean SendAcksAsync { get; set; } = true;

        /// <summary>
        ///     Gets or sets the dispatch async option.
        /// </summary>
        /// <value>The dispatch async option.</value>
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

        /// <summary>
        ///     Gets or sets the STOMP producer settings.
        /// </summary>
        /// <value>The STOMP producer settings.</value>
        public StompProducerSettings ProducerSettings { get; } = new StompProducerSettings();

        #endregion
    }
}