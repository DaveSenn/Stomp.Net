namespace Stomp.Net
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
}