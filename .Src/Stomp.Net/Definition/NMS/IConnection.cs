#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Represents a connection with a message broker
    /// </summary>
    public interface IConnection : IDisposable, IStartStoppable
    {
        /// <summary>
        ///     Closes the connection.
        /// </summary>
        [PublicAPI]
        void Close();

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        [PublicAPI]
        ISession CreateSession();

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        [PublicAPI]
        ISession CreateSession( AcknowledgementMode acknowledgementMode );

        /// <summary>
        ///     An asynchronous listener which can be notified if an error occurs
        /// </summary>
        [PublicAPI]
        event Action<Exception> ExceptionListener;

        #region Connection Management methods

        /// <summary>
        ///     For a long running Connection that creates many temp destinations
        ///     this method will close and destroy all previously created temp
        ///     destinations to reduce resource consumption.  This can be useful
        ///     when the Connection is pooled or otherwise used for long periods
        ///     of time.  Only locally created temp destinations should be removed
        ///     by this call.
        ///     NOTE: This is an optional operation and for NMS providers that
        ///     do not support this functionality the method should just return
        ///     without throwing any exceptions.
        /// </summary>
        [PublicAPI]
        void PurgeTempDestinations();

        #endregion

        #region Attributes

        /// <summary>
        ///     Sets the unique client ID for this connection before Start() or returns the
        ///     unique client ID after the connection has started
        /// </summary>
        [PublicAPI]
        String ClientId { get; set; }

        /// <summary>
        ///     Get/or set the redelivery policy for this connection.
        /// </summary>
        [PublicAPI]
        IRedeliveryPolicy RedeliveryPolicy { get; set; }

        #endregion
    }
}