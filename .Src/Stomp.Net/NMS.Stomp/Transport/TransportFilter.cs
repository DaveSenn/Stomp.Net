

#region Usings

using System;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     Used to implement a filter on the transport layer.
    /// </summary>
    public class TransportFilter : ITransport
    {
        #region Fields

        protected readonly ITransport next;
        protected CommandHandler commandHandler;
        protected ExceptionHandler exceptionHandler;
        protected InterruptedHandler interruptedHandler;
        protected ResumedHandler resumedHandler;

        #endregion

        #region Ctor

        public TransportFilter( ITransport next )
        {
            this.next = next;
            this.next.Command = OnCommand;
            this.next.Exception = OnException;
            this.next.Interrupted = OnInterrupted;
            this.next.Resumed = OnResumed;
        }

        #endregion

        /// <summary>
        ///     Method Dispose
        /// </summary>
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
            get { return next.IsStarted; }
        }

        /// <summary>
        ///     Method Start
        /// </summary>
        public virtual void Start()
        {
            if ( commandHandler == null )
                throw new InvalidOperationException( "command cannot be null when Start is called." );

            if ( exceptionHandler == null )
                throw new InvalidOperationException( "exception cannot be null when Start is called." );

            next.Start();
        }

        public virtual void Stop()
        {
            next.Stop();
        }

        /// <summary>
        ///     Method AsyncRequest
        /// </summary>
        /// <returns>A FutureResponse</returns>
        /// <param name="command">A  Command</param>
        public virtual FutureResponse AsyncRequest( Command command )
        {
            return next.AsyncRequest( command );
        }

        /// <summary>
        ///     Timeout in milliseconds to wait for sending asynchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 AsyncTimeout
        {
            get { return next.AsyncTimeout; }
            set { next.AsyncTimeout = value; }
        }

        public CommandHandler Command
        {
            get { return commandHandler; }
            set { commandHandler = value; }
        }

        public ExceptionHandler Exception
        {
            get { return exceptionHandler; }
            set { exceptionHandler = value; }
        }

        public InterruptedHandler Interrupted
        {
            get { return interruptedHandler; }
            set { interruptedHandler = value; }
        }

        public Boolean IsConnected
        {
            get { return next.IsConnected; }
        }

        public Boolean IsDisposed { get; private set; }

        public Boolean IsFaultTolerant
        {
            get { return next.IsFaultTolerant; }
        }

        public Object Narrow( Type type )
        {
            if ( GetType()
                .Equals( type ) )
                return this;
            if ( next != null )
                return next.Narrow( type );

            return null;
        }

        /// <summary>
        ///     Method Oneway
        /// </summary>
        /// <param name="command">A  Command</param>
        public virtual void Oneway( Command command )
        {
            next.Oneway( command );
        }

        public Uri RemoteAddress
        {
            get { return next.RemoteAddress; }
        }

        /// <summary>
        ///     Method Request
        /// </summary>
        /// <returns>A Response</returns>
        /// <param name="command">A  Command</param>
        public virtual Response Request( Command command )
        {
            return Request( command, TimeSpan.FromMilliseconds( System.Threading.Timeout.Infinite ) );
        }

        /// <summary>
        ///     Method Request with time out for Response.
        /// </summary>
        /// <returns>A Response</returns>
        /// <param name="command">A  Command</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public virtual Response Request( Command command, TimeSpan timeout )
        {
            return next.Request( command, timeout );
        }

        public ResumedHandler Resumed
        {
            get { return resumedHandler; }
            set { resumedHandler = value; }
        }

        /// <summary>
        ///     Timeout in milliseconds to wait for sending synchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 Timeout
        {
            get { return next.Timeout; }
            set { next.Timeout = value; }
        }

        protected virtual void Dispose( Boolean disposing )
        {
            if ( disposing )
            {
                Tracer.Debug( "TransportFilter disposing of next Transport: " +
                              next.GetType()
                                  .Name );
                next.Dispose();
            }
            IsDisposed = true;
        }

        protected virtual void OnCommand( ITransport sender, Command command )
        {
            commandHandler( sender, command );
        }

        protected virtual void OnException( ITransport sender, Exception command )
        {
            exceptionHandler( sender, command );
        }

        protected virtual void OnInterrupted( ITransport sender )
        {
            if ( interruptedHandler != null )
                interruptedHandler( sender );
        }

        protected virtual void OnResumed( ITransport sender )
        {
            if ( resumedHandler != null )
                resumedHandler( sender );
        }

        ~TransportFilter()
        {
            Dispose( false );
        }
    }
}