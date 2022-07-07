#region Usings

using System;

#endregion

namespace Stomp.Net;

/// <summary>
///     Class representing STOMP SSL settings.
/// </summary>
public class StompSslSettings
{
    #region Properties

    /// <summary>
    ///     Gets or sets the server name.
    /// </summary>
    /// <value>The server name.</value>
    public String ServerName { get; set; }

    /// <summary>
    ///     Gets or sets the client certificate subject.
    /// </summary>
    /// <value>The client certificate subject.</value>
    public String ClientCertSubject { get; set; }

    /// <summary>
    ///     Gets or sets the client certificate filename.
    /// </summary>
    /// <value>The client certificate filename.</value>
    public String ClientCertFilename { get; set; }

    /// <summary>
    ///     Gets or sets the client certificate private key filename.
    /// </summary>
    /// <value>The client certificate private key filename.</value>
    public String ClientCertKeyFilename { get; set; }

    /// <summary>
    ///     Gets or sets the client certificate password.
    /// </summary>
    /// <value>The client certificate password.</value>
    public String ClientCertPassword { get; set; }

    /// <summary>
    ///     Gets or sets a value determining whether invalid broker certificates should be accepted or not.
    /// </summary>
    /// <value>A value determining whether invalid broker certificates should be accepted or not.</value>
    public Boolean AcceptInvalidBrokerCert { get; set; } = false;

    /// <summary>
    ///     Gets or sets the key store name.
    /// </summary>
    /// <value>The key store name.</value>
    public String KeyStoreName { get; set; }

    /// <summary>
    ///     Gets or sets the key store location.
    /// </summary>
    /// <value>The key store location.</value>
    public String KeyStoreLocation { get; set; }

    #endregion
}