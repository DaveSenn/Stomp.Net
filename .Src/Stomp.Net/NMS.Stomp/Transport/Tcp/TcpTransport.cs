#region Usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Util;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    /// <summary>
    ///     An implementation of ITransport that uses sockets to communicate with the broker
    /// </summary>
    public class TcpTransport : Disposable, ITransport
    {
        #region Fields

        private readonly Atomic<Boolean> _closed = new Atomic<Boolean>( false );

        /// <summary>
        ///     Object used to synchronize threads for start and stop logic.
        /// </summary>
        private readonly Object _startStopLock = new Object();

        /// <summary>
        /// Reader for the transport socket.
        /// </summary>
        private BinaryReader _socketReader;
        /// <summary>
        /// Writer for the transport socket.
        /// </summary>
        private BinaryWriter _socketWriter;

        protected readonly Socket Socket;
        private TimeSpan _maxThreadWait = TimeSpan.FromMilliseconds( 30000 );
        private Thread _readThread;
        private volatile Boolean _seenShutdown;
        private Boolean _started;

        #endregion

        #region Properties

        public Boolean TcpNoDelayEnabled
        {
#if !NETCF
            get { return Socket.NoDelay; }
            set { Socket.NoDelay = value; }
#else
            get { return false; }
            set { }
#endif
        }

        public IWireFormat Wireformat { get; set; }

        #endregion

        #region Ctor

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

        public Action<ITransport, ICommand> Command { get; set; }

        public ExceptionHandler Exception { get; set; }

        public InterruptedHandler Interrupted { get; set; }

        public ResumedHandler Resumed { get; set; }

        /// <summary>
        ///     Timeout in milliseconds to wait for sending synchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 Timeout { get; set; } = -1;

        /// <summary>
        ///     Timeout in milliseconds to wait for sending asynchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 AsyncTimeout { get; set; } = -1;

        public Uri RemoteAddress { get; }

        public Boolean IsConnected
        {
            get { return Socket.Connected; }
        }

        public Boolean IsFaultTolerant
        {
            get { return false; }
        }

        public Object Narrow( Type type )
        {
            if ( GetType()
                .Equals( type ) )
                return this;

            return null;
        }

        public void Oneway( ICommand command )
        {
            lock ( _startStopLock )
            {
                if ( _closed.Value )
                    throw new InvalidOperationException( "Error writing to broker.  Transport connection is closed." );

                if ( command is ShutdownInfo )
                    _seenShutdown = true;

                Wireformat.Marshal( command, _socketWriter );
            }
        }

        public FutureResponse AsyncRequest( ICommand command )
        {
            throw new NotImplementedException( "Use a ResponseCorrelator if you want to issue AsyncRequest calls" );
        }

        public Response Request( ICommand command, TimeSpan timeout )
        {
            throw new NotImplementedException( "Use a ResponseCorrelator if you want to issue Request calls" );
        }

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
                        throw new InvalidOperationException( $"{nameof( Command )} cannot be null when Start is called." );

                    if ( null == Exception )
                        throw new InvalidOperationException( $"{nameof( Exception )} cannot be null when Start is called." );

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

        private void Close()
        {
            Thread theReadThread = null;

            if ( _closed.CompareAndSet( false, true ) )
                lock ( _startStopLock )
                {
                    try
                    {
                        Socket.Shutdown( SocketShutdown.Both );
                    }
                    catch
                    {
                    }

                    try
                    {
                        if ( null != _socketWriter )
                            _socketWriter.Close();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _socketWriter = null;
                    }

                    try
                    {
                        if ( null != _socketReader )
                            _socketReader.Close();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _socketReader = null;
                    }

                    try
                    {
                        Socket.Close();
                    }
                    catch
                    {
                    }

                    theReadThread = _readThread;
                    _readThread = null;
                    _started = false;
                }

            if ( null != theReadThread )
                try
                {
                    if ( Thread.CurrentThread != theReadThread
#if !NETCF
                         && theReadThread.IsAlive
#endif
                    )
                        if ( !theReadThread.Join( (Int32) _maxThreadWait.TotalMilliseconds ) )
                            theReadThread.Abort();
                }
                catch
                {
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
                    command = (ICommand) Wireformat.Unmarshal( _socketReader );
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