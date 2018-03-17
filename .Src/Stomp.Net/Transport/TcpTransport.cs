#region Usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Stomp.Transport;
using Stomp.Net.Utilities;

#endregion

namespace Stomp.Net.Transport
{
    /// <summary>
    ///     An implementation of ITransport that uses sockets to communicate with the broker
    /// </summary>
    public class TcpTransport : Disposable, ITransport
    {
        #region Fields

        /// <summary>
        ///     Stores whether the connection is closed or not.
        /// </summary>
        private readonly Atomic<Boolean> _closed = new Atomic<Boolean>( false );

        /// <summary>
        ///     Object used to synchronize threads for start and stop logic.
        /// </summary>
        private readonly Object _startStopLock = new Object();

        /// <summary>
        ///     The socket used for the network communication.
        /// </summary>
        protected readonly Socket Socket;

        /// <summary>
        ///     Timeout for closing the connection.
        /// </summary>
        private TimeSpan _maxThreadWait = TimeSpan.FromMilliseconds( 30000 );

        /// <summary>
        ///     Reading thread (background).
        /// </summary>
        private Thread _readThread;

        /// <summary>
        ///     Stores whether a shutdown has happened or not.
        /// </summary>
        private volatile Boolean _seenShutdown;

        /// <summary>
        ///     Reader for the transport socket.
        /// </summary>
        private BinaryReader _socketReader;

        /// <summary>
        ///     Writer for the transport socket.
        /// </summary>
        private BinaryWriter _socketWriter;

        /// <summary>
        ///     Stores whether the connection is started or not.
        /// </summary>
        private Boolean _started;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a <see cref="IWireFormat" />.
        /// </summary>
        /// <value>A <see cref="IWireFormat" />.</value>
        private IWireFormat Wireformat { get; }

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TcpTransport" /> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="socket">The socket to use.</param>
        /// <param name="wireformat">A <see cref="IWireFormat" />.</param>
        public TcpTransport( Uri uri, Socket socket, IWireFormat wireformat )
        {
            RemoteAddress = uri;
            Socket = socket;
            Wireformat = wireformat;
        }

        #endregion

        #region Protected Members

        /// <summary>
        ///     Creates a stream for the transport socket.
        /// </summary>
        /// <returns>Returns the newly created stream.</returns>
        protected virtual Stream CreateSocketStream()
            => new NetworkStream( Socket );

        #endregion

        #region Override of Disposable

        /// <summary>
        ///     Method invoked when the instance gets disposed.
        /// </summary>
        protected override void Disposed()
            => Close();

        #endregion

        #region Implementation of ITransport

        /// <summary>
        ///     Delegate invoked when a command was received.
        /// </summary>
        public Action<ITransport, ICommand> Command { get; set; }

        /// <summary>
        ///     Delegate invoked when a exception occurs.
        /// </summary>
        public Action<ITransport, Exception> Exception { get; set; }

        /// <summary>
        ///     Gets or sets the timeout for sending synchronous messages or commands.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.Zero;

        /// <summary>
        ///     Gets or sets the timeout for sending asynchronous messages or commands.
        /// </summary>
        public TimeSpan AsyncTimeout { get; set; } = TimeSpan.Zero;

        /// <value>
        ///     The Remote Address that this transport is currently connected to.
        /// </value>
        public Uri RemoteAddress { get; }

        /// <value>
        ///     Indicates if the Transport is current Connected to is assigned URI.
        /// </value>
        public Boolean IsConnected => Socket.Connected;

        /// <value>
        ///     Indicates if this Transport is Fault Tolerant or not.
        ///     A fault Tolerant Transport handles low level connection errors internally allowing a client to remain unaware of
        ///     wire level disconnection and reconnection details.
        /// </value>
        public Boolean IsFaultTolerant => false;

        /// <summary>
        ///     Allows a caller to find a specific type of Transport in the Chain of
        ///     Transports that is created.
        ///     This allows a caller to find a specific object in the Transport chain and set or get properties on that specific
        ///     instance.
        ///     If the requested type isn't in the chain than Null is returned.
        /// </summary>
        public Object Narrow( Type type )
            => GetType() == type ? this : null;

        /// <summary>
        ///     Sends a Command object on the Wire but does not wait for any response from the receiver before returning.
        /// </summary>
        /// <param name="command">
        ///     A <see cref="Command" />
        /// </param>
        public void Oneway( ICommand command )
        {
            lock ( _startStopLock )
            {
                if ( _closed.Value )
                    throw new InvalidOperationException( "Error writing to broker. Transport connection is closed." );

                if ( command is ShutdownInfo )
                    _seenShutdown = true;

                Wireformat.Marshal( command, _socketWriter );
            }
        }

        /// <summary>
        ///     Sends a Command object which requires a response from the Broker but does not
        ///     wait for the response, instead a FutureResponse object is returned that the
        ///     caller can use to wait on the Broker's response.
        /// </summary>
        public FutureResponse AsyncRequest( ICommand command ) =>
            throw new NotImplementedException( "Use a ResponseCorrelator if you want to issue AsyncRequest calls" );

        /// <summary>
        ///     Sends a Command to the Broker and waits for the given TimeSpan to expire for a response before returning.
        /// </summary>
        public Response Request( ICommand command, TimeSpan timeout )
            => throw new NotImplementedException( "Use a ResponseCorrelator if you want to issue Request calls" );

        #endregion

        #region Implementation of IStartStoppable

        /// <summary>
        ///     Gets a value indicating whether the object is started or not.
        /// </summary>
        /// <value>A value indicating whether the object is started or not.</value>
        public Boolean IsStarted
        {
            get
            {
                lock ( _startStopLock )
                    return _started;
            }
        }

        /// <summary>
        ///     Starts the object, if not yet started.
        /// </summary>
        public void Start()
        {
            lock ( _startStopLock )
                if ( !_started )
                {
                    if ( null == Command )
                        throw new InvalidOperationException( $"{nameof(Command)} cannot be null when Start is called." );

                    if ( null == Exception )
                        throw new InvalidOperationException( $"{nameof(Exception)} cannot be null when Start is called." );

                    // Initialize our Read and Writer instances.
                    // Its not actually necessary to have two distinct NetworkStream instances but for now the TcpTransport
                    // will continue to do so for legacy reasons.
                    _socketWriter = new BinaryWriter( CreateSocketStream() );
                    _socketReader = new BinaryReader( CreateSocketStream() );

                    // Now lets create the background read thread
                    _readThread = new Thread( ReadLoop ) { IsBackground = true };
                    _readThread.Start();

                    _started = true;
                }
        }

        /// <summary>
        ///     Stops the object.
        /// </summary>
        public void Stop()
            => Close();

        #endregion

        #region Private Members

        /// <summary>
        ///     Closes the network connection.
        /// </summary>
        private void Close()
        {
            Thread theReadThread = null;

            if ( _closed.CompareAndSet( false, true ) )
                lock ( _startStopLock )
                {
                    try
                    {
                        Socket?.Shutdown( SocketShutdown.Both );
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        _socketWriter?.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        _socketWriter = null;
                    }

                    try
                    {
                        _socketReader?.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        _socketReader = null;
                    }

                    try
                    {
                        Socket?.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }

                    theReadThread = _readThread;
                    _readThread = null;
                    _started = false;
                }

            if ( null == theReadThread )
                return;

            try
            {
                if ( Thread.CurrentThread == theReadThread || !theReadThread.IsAlive )
                    return;

                if ( !theReadThread.Join( (Int32) _maxThreadWait.TotalMilliseconds ) )
                    Tracer.Warn( "Joining thread timed out." );
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     This is the thread function for the reader thread. This runs continuously performing a blocking read on the socket
        ///     and dispatching all commands received.
        /// </summary>
        /// <remarks>
        ///     Exception Handling
        ///     ------------------
        ///     If an Exception occurs during the reading/marshaling, then the connection is effectively broken because position
        ///     cannot be re-established to the next message. This is reported to the application via the exceptionHandler and the
        ///     socket is closed to prevent further communication attempts.
        ///     An exception in the command handler may not be fatal to the transport, so these are simply reported to the
        ///     exceptionHandler.
        /// </remarks>
        private void ReadLoop()
        {
            while ( !_closed.Value )
            {
                ICommand command;

                try
                {
                    command = Wireformat.Unmarshal( _socketReader );
                }
                catch ( Exception ex )
                {
                    if ( !_closed.Value )
                    {
                        // Close the socket as there's little that can be done with this transport now.
                        Close();

                        if ( !_seenShutdown )
                            Exception( this, ex );
                    }

                    break;
                }

                try
                {
                    if ( command != null )
                        Command( this, command );
                }
                catch ( Exception ex )
                {
                    Exception( this, ex );
                }
            }
        }

        #endregion
    }
}