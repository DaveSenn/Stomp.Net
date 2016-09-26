

#region Usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Transport.Tcp
{
    /// <summary>
    ///     An implementation of ITransport that uses sockets to communicate with the broker
    /// </summary>
    public class TcpTransport : ITransport
    {
        #region Fields

        private readonly Atomic<Boolean> closed = new Atomic<Boolean>( false );
        protected readonly Object myLock = new Object();
        protected readonly Socket socket;

        private TimeSpan MAX_THREAD_WAIT = TimeSpan.FromMilliseconds( 30000 );
        private Thread readThread;
        private volatile Boolean seenShutdown;
        private BinaryReader socketReader;
        private BinaryWriter socketWriter;
        private Boolean started;

        #endregion

        #region Properties

        public Boolean TcpNoDelayEnabled
        {
#if !NETCF
            get { return socket.NoDelay; }
            set { socket.NoDelay = value; }
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
            this.socket = socket;
            Wireformat = wireformat;
        }

        #endregion

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        ///     Property IsStarted
        /// </summary>
        public Boolean IsStarted
        {
            get
            {
                lock ( myLock )
                    return started;
            }
        }

        /// <summary>
        ///     Method Start
        /// </summary>
        public void Start()
        {
            lock ( myLock )
                if ( !started )
                {
                    if ( null == Command )
                        throw new InvalidOperationException(
                            "command cannot be null when Start is called." );

                    if ( null == Exception )
                        throw new InvalidOperationException(
                            "exception cannot be null when Start is called." );

                    started = true;

                    // Initialize our Read and Writer instances.  Its not actually necessary
                    // to have two distinct NetworkStream instances but for now the TcpTransport
                    // will continue to do so for legacy reasons.
                    socketWriter = new BinaryWriter( CreateSocketStream() );
                    socketReader = new BinaryReader( CreateSocketStream() );

                    // now lets create the background read thread
                    readThread = new Thread( ReadLoop );
                    readThread.IsBackground = true;
                    readThread.Start();
                }
        }

        public void Stop() => Close();

        public FutureResponse AsyncRequest( Command command )
        {
            throw new NotImplementedException( "Use a ResponseCorrelator if you want to issue AsyncRequest calls" );
        }

        /// <summary>
        ///     Timeout in milliseconds to wait for sending asynchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 AsyncTimeout { get; set; } = -1;

        public CommandHandler Command { get; set; }

        public ExceptionHandler Exception { get; set; }

        public InterruptedHandler Interrupted { get; set; }

        public Boolean IsConnected
        {
            get { return socket.Connected; }
        }

        public Boolean IsDisposed { get; private set; }

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

        public void Oneway( Command command )
        {
            lock ( myLock )
            {
                if ( closed.Value )
                    throw new InvalidOperationException( "Error writing to broker.  Transport connection is closed." );

                if ( command is ShutdownInfo )
                    seenShutdown = true;

                Wireformat.Marshal( command, socketWriter );
            }
        }

        public Uri RemoteAddress { get; }

        public Response Request( Command command )
        {
            throw new NotImplementedException( "Use a ResponseCorrelator if you want to issue Request calls" );
        }

        public Response Request( Command command, TimeSpan timeout )
        {
            throw new NotImplementedException( "Use a ResponseCorrelator if you want to issue Request calls" );
        }

        public ResumedHandler Resumed { get; set; }

        // Implementation methods

        /// <summary>
        ///     Timeout in milliseconds to wait for sending synchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 Timeout { get; set; } = -1;

        public void Close()
        {
            Thread theReadThread = null;

            if ( closed.CompareAndSet( false, true ) )
                lock ( myLock )
                {
                    try
                    {
                        socket.Shutdown( SocketShutdown.Both );
                    }
                    catch
                    {
                    }

                    try
                    {
                        if ( null != socketWriter )
                            socketWriter.Close();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        socketWriter = null;
                    }

                    try
                    {
                        if ( null != socketReader )
                            socketReader.Close();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        socketReader = null;
                    }

                    try
                    {
                        socket.Close();
                    }
                    catch
                    {
                    }

                    theReadThread = readThread;
                    readThread = null;
                    started = false;
                }

            if ( null != theReadThread )
                try
                {
                    if ( Thread.CurrentThread != theReadThread
#if !NETCF
                         && theReadThread.IsAlive
#endif
                    )
                        if ( !theReadThread.Join( (Int32) MAX_THREAD_WAIT.TotalMilliseconds ) )
                            theReadThread.Abort();
                }
                catch
                {
                }
        }

        public void ReadLoop()
        {
            // This is the thread function for the reader thread. This runs continuously
            // performing a blokcing read on the socket and dispatching all commands
            // received.
            //
            // Exception Handling
            // ------------------
            // If an Exception occurs during the reading/marshalling, then the connection
            // is effectively broken because position cannot be re-established to the next
            // message.  This is reported to the app via the exceptionHandler and the socket
            // is closed to prevent further communication attempts.
            //
            // An exception in the command handler may not be fatal to the transport, so
            // these are simply reported to the exceptionHandler.
            //
            while ( !closed.Value )
            {
                Command command = null;

                try
                {
                    command = (Command) Wireformat.Unmarshal( socketReader );
                }
                catch ( Exception ex )
                {
                    command = null;
                    if ( !closed.Value )
                    {
                        // Close the socket as there's little that can be done with this transport now.
                        Close();
                        if ( !seenShutdown )
                            Exception( this, ex );
                    }

                    break;
                }

                try
                {
                    if ( command != null )
                        Command( this, command );
                }
                catch ( Exception e )
                {
                    Exception( this, e );
                }
            }
        }

        protected virtual Stream CreateSocketStream() => new NetworkStream( socket );

        protected void Dispose( Boolean disposing )
        {
            Close();
            IsDisposed = true;
        }

        ~TcpTransport()
        {
            Dispose( false );
        }
    }
}