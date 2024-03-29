#region Usings

using System;
using Stomp.Net.Stomp;
using Stomp.Net.Stomp.Util;

#endregion

namespace Stomp.Net;

/// <summary>
///     STOMP connection settings.
/// </summary>
public class StompConnectionSettings
{
    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether the destination name formatting should be skipped or not.
    ///     If set to true the physical name property will be used as stomp destination string without adding prefixes such as
    ///     queue or topic. This to support JMS brokers listening for queue/topic names in a different format.
    /// </summary>
    public Boolean SkipDestinationNameFormatting { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the host header will be set or not.
    /// </summary>
    /// <remarks>
    ///     Disabling the host header can make sens if you are working with a broker like RabbitMq
    ///     which uses the host header as name of the target virtual host.
    ///     Default is
    ///     <value>true</value>
    ///     .
    /// </remarks>
    public Boolean SetHostHeader { get; set; } = true;

    /// <summary>
    ///     Gets or sets the value used as host header.
    ///     If set Stomp.Net will use this value as content of the host header.
    /// </summary>
    public String HostHeaderOverride { get; set; }

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
    public PrefetchPolicy PrefetchPolicy { get; } = new();

    /// <summary>
    ///     Gets the transport settings.
    /// </summary>
    /// <value>The transport settings.</value>
    public StompTransportSettings TransportSettings { get; } = new();

    /// <summary>
    ///     Gets or sets the STOMP producer settings.
    /// </summary>
    /// <value>The STOMP producer settings.</value>
    public StompProducerSettings ProducerSettings { get; } = new();

    #endregion
}