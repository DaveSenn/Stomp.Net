#region Usings

using System;
using Apache.NMS.Stomp.Commands;
using Stomp.Net;

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

        #endregion

        #region Properties

        public Boolean IsDisposed { get; private set; }

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
        public Boolean IsStarted => next.IsStarted;

        /// <summary>
        ///     Method Start
        /// </summary>
        public virtual void Start()
        {
            if ( Command == null )
                throw new InvalidOperationException( "command cannot be null when Start is called." );

            if ( Exception == null )
                throw new InvalidOperationException( "exception cannot be null when Start is called." );

            next.Start();
        }

        public virtual void Stop()
            => next.Stop();

        /// <summary>
        ///     Method AsyncRequest
        /// </summary>
        /// <returns>A FutureResponse</returns>
        /// <param name="command">A  Command</param>
        public virtual FutureResponse AsyncRequest( ICommand command )
            => next.AsyncRequest( command );

        /// <summary>
        ///     Timeout in milliseconds to wait for sending asynchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 AsyncTimeout
        {
            get { return next.AsyncTimeout; }
            set { next.AsyncTimeout = value; }
        }

        /// <summary>
        ///     Delegate invoked when the connection is interrupted.
        /// </summary>
        public Action<ITransport> Interrupted { get; set; }

        public Boolean IsConnected => next.IsConnected;

        public Boolean IsFaultTolerant => next.IsFaultTolerant;

        public Object Narrow( Type type )
            => GetType() == type ? this : next?.Narrow( type );

        /// <summary>
        ///     Method Oneway
        /// </summary>
        /// <param name="command">A  Command</param>
        public virtual void Oneway( ICommand command ) => next.Oneway( command );

        public Uri RemoteAddress => next.RemoteAddress;

        /// <summary>
        ///     Method Request with time out for Response.
        /// </summary>
        /// <returns>A Response</returns>
        /// <param name="command">A  Command</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public virtual Response Request( ICommand command, TimeSpan timeout ) => next.Request( command, timeout );

        /// <summary>
        ///     Delegate invoked when the connection is resumed.
        /// </summary>
        public Action<ITransport> Resumed { get; set; }

        /// <summary>
        ///     Timeout in milliseconds to wait for sending synchronous messages or commands.
        ///     Set to -1 for infinite timeout.
        /// </summary>
        public Int32 Timeout
        {
            get { return next.Timeout; }
            set { next.Timeout = value; }
        }

        /// <summary>
        ///     Method Request
        /// </summary>
        /// <returns>A Response</returns>
        /// <param name="command">A  Command</param>
        public virtual Response Request( ICommand command ) => Request( command, TimeSpan.FromMilliseconds( System.Threading.Timeout.Infinite ) );

        protected virtual void Dispose( Boolean disposing )
        {
            if ( disposing )
                next.Dispose();

            IsDisposed = true;
        }

        /// <summary>
        ///     Invokes the command delegate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="command">The command.</param>
        protected virtual void OnCommand( ITransport sender, ICommand command )
            => Command( sender, command );

        /// <summary>
        ///     Invokes the exception delegate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="command">The command.</param>
        protected virtual void OnException( ITransport sender, Exception command )
            => Exception( sender, command );

        /// <summary>
        ///     Invokes the interrupted delegate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void OnInterrupted( ITransport sender )
            => Interrupted?.Invoke( sender );

        /// <summary>
        ///     Invokes the resumed delegate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void OnResumed( ITransport sender )
            => Resumed?.Invoke( sender );

        ~TransportFilter()
        {
            Dispose( false );
        }

        /*
        protected CommandHandler commandHandler;
        protected ExceptionHandler exceptionHandler;
        protected InterruptedHandler interruptedHandler;
        protected ResumedHandler resumedHandler;
        */

        #region Implementation of ITransport

        /// <summary>
        ///     Delegate invoked when a command was received.
        /// </summary>
        public Action<ITransport, ICommand> Command { get; set; }

        /// <summary>
        ///     Delegate invoked when a exception occurs.
        /// </summary>
        public Action<ITransport, Exception> Exception { get; set; }

        #endregion
    }
}