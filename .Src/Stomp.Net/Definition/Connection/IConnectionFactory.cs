#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     A Factory of IConnection objects
    /// </summary>
    public interface IConnectionFactory
    {
        #region Properties

        /// <summary>
        ///     Get/or set the broker Uri.
        /// </summary>
        Uri BrokerUri { get; set; }

        /// <summary>
        ///     Get or set the redelivery policy that new IConnection objects are assigned upon creation.
        /// </summary>
        IRedeliveryPolicy RedeliveryPolicy { get; set; }

        #endregion

        /// <summary>
        ///     Creates a new connection
        /// </summary>
        IConnection CreateConnection();
    }
}