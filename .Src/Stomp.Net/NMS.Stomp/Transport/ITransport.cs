#region Usings

using System;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    // public delegate void CommandHandler( ITransport sender, ICommand command );

    public delegate void ExceptionHandler( ITransport sender, Exception command );

    public delegate void InterruptedHandler( ITransport sender );

    public delegate void ResumedHandler( ITransport sender );

    /// <summary>
    ///     Represents the logical networking transport layer.
    ///     Transports implement the low level protocol specific portion of the Communication between the Client and a Broker
    ///     such as TCP, UDP, etc.  Transports make use of WireFormat objects to handle translating
    ///     the canonical OpenWire Commands used in this client into binary wire level packets that
    ///     can be sent to the Broker or Service that the Transport connects to.
    /// </summary>
    public interface ITransport : IStartStoppable, IDisposable
    {
        #region Properties

        /// <summary>
        ///     Timeout in milliseconds to wait for sending synchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        Int32 Timeout { get; set; }

        /// <summary>
        ///     Timeout in milliseconds to wait for sending asynchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        Int32 AsyncTimeout { get; set; }

        Action<ITransport, ICommand> Command { get; set; }

        ExceptionHandler Exception { get; set; }

        InterruptedHandler Interrupted { get; set; }

        ResumedHandler Resumed { get; set; }

        /// <value>
        ///     Indicates if this Transport is Fault Tolerant or not.
        ///     A fault Tolerant Transport handles low level connection errors internally allowing a client to remain unaware of
        ///     wire level disconnection and reconnection details.
        /// </value>
        Boolean IsFaultTolerant { get; }

        /// <value>
        ///     Indicates if the Transport is current Connected to is assigned URI.
        /// </value>
        Boolean IsConnected { get; }

        /// <value>
        ///     The Remote Address that this transport is currently connected to.
        /// </value>
        Uri RemoteAddress { get; }

        #endregion

        /// <summary>
        ///     Sends a Command object which requires a response from the Broker but does not
        ///     wait for the response, instead a FutureResponse object is returned that the
        ///     caller can use to wait on the Broker's response.
        /// </summary>
        FutureResponse AsyncRequest( ICommand command );

        /// <summary>
        ///     Allows a caller to find a specific type of Transport in the Chain of
        ///     Transports that is created.
        ///     This allows a caller to find a specific object in the Transport chain and set or get properties on that specific
        ///     instance.
        ///     If the requested type isn't in the chain than Null is returned.
        /// </summary>
        Object Narrow( Type type );

        /// <summary>
        ///     Sends a Command object on the Wire but does not wait for any response from the receiver before returning.
        /// </summary>
        /// <param name="command">
        ///     A <see cref="Command" />
        /// </param>
        void Oneway( ICommand command );

        /// <summary>
        ///     Sends a Command to the Broker and waits for the given TimeSpan to expire for a response before returning.
        /// </summary>
        Response Request( ICommand command, TimeSpan timeout );
    }
}