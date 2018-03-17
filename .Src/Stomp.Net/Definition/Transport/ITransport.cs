#region Usings

using System;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Stomp.Transport;

#endregion

namespace Stomp.Net
{
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
        ///     Gets or sets the timeout for sending synchronous messages or commands.
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        ///     Gets or sets the timeout for sending asynchronous messages or commands.
        /// </summary>
        TimeSpan AsyncTimeout { get; set; }

        /// <summary>
        ///     Delegate invoked when a command was received.
        /// </summary>
        Action<ITransport, ICommand> Command { get; set; }

        /// <summary>
        ///     Delegate invoked when a exception occurs.
        /// </summary>
        Action<ITransport, Exception> Exception { get; set; }

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