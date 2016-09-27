#region Usings

using System;
using Stomp.Net;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     The mode used to acknowledge messages after they are consumed
    /// </summary>
    public enum AcknowledgementMode
    {
        /// <summary>
        ///     With this acknowledgment mode, the session will not
        ///     acknowledge receipt of a message since the broker assumes
        ///     successful receipt of a message after the onMessage handler
        ///     has returned without error.
        /// </summary>
        AutoAcknowledge,

        /// <summary>
        ///     With this acknowledgment mode, the session automatically
        ///     acknowledges a client's receipt of a message either when
        ///     the session has successfully returned from a call to receive
        ///     or when the message listener the session has called to
        ///     process the message successfully returns.  Acknowlegements
        ///     may be delayed in this mode to increase performance at
        ///     the cost of the message being redelivered this client fails.
        /// </summary>
        DupsOkAcknowledge,

        /// <summary>
        ///     With this acknowledgment mode, the client acknowledges a
        ///     consumed message by calling the message's acknowledge method.
        ///     This acknowledgement acknowledges the given message and all
        ///     unacknowedged messages that have preceeded it for the session
        ///     in which the message was delivered.
        /// </summary>
        ClientAcknowledge,

        /// <summary>
        ///     Messages will be consumed when the transaction commits.
        /// </summary>
        Transactional,

        /// <summary>
        ///     With this acknowledgment mode, the client acknowledges a
        ///     consumed message by calling the message's acknowledge method.
        ///     This acknowledgement mode allows the client to acknowledge a
        ///     single message.  This mode is not required to be supported by
        ///     all NMS providers, however the provider should throw an appropriate
        ///     exception to indicate that the mode is unsupported.
        /// </summary>
        IndividualAcknowledge
    }

    /// <summary>
    ///     A delegate that can receive transport level exceptions.
    /// </summary>
    public delegate void ExceptionListener( Exception exception );

    /// <summary>
    ///     A delegate that is used by Fault tolerant NMS Implementation to notify their
    ///     clients that the Connection is not currently active to due some error.
    /// </summary>
    public delegate void ConnectionInterruptedListener();

    /// <summary>
    ///     A delegate that is used by Fault tolerant NMS Implementation to notify their
    ///     clients that the Connection that was interrupted has now been restored.
    /// </summary>
    public delegate void ConnectionResumedListener();

    /// <summary>
    ///     Represents a connection with a message broker
    /// </summary>
    public interface IConnection : IDisposable, IStartStoppable
    {
        /// <summary>
        ///     Closes the connection.
        /// </summary>
        void Close();

        /// <summary>
        ///     An asynchronous listener that is notified when a Fault tolerant connection
        ///     has been interrupted.
        /// </summary>
        event ConnectionInterruptedListener ConnectionInterruptedListener;

        /// <summary>
        ///     An asynchronous listener that is notified when a Fault tolerant connection
        ///     has been resumed.
        /// </summary>
        event ConnectionResumedListener ConnectionResumedListener;

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        ISession CreateSession();

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        ISession CreateSession( AcknowledgementMode acknowledgementMode );

        /// <summary>
        ///     An asynchronous listener which can be notified if an error occurs
        /// </summary>
        event ExceptionListener ExceptionListener;

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
        void PurgeTempDestinations();

        #endregion

        #region Attributes

        /// <summary>
        ///     Sets the unique client ID for this connection before Start() or returns the
        ///     unique client ID after the connection has started
        /// </summary>
        String ClientId { get; set; }

        /// <summary>
        ///     Get/or set the redelivery policy for this connection.
        /// </summary>
        IRedeliveryPolicy RedeliveryPolicy { get; set; }

        #endregion
    }
}