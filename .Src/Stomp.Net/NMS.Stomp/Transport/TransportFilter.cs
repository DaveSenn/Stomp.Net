#region Usings

using System;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp.Transport
{
    /// <summary>
    ///     Used to implement a filter on the transport layer.
    /// </summary>
    public abstract class TransportFilter : Disposable, ITransport
    {
        #region Fields

        protected readonly ITransport Next;

        #endregion

        #region Ctor

        protected TransportFilter( ITransport next )
        {
            Next = next;
            Next.Command = OnCommand;
            Next.Exception = OnException;
        }

        #endregion

        /// <summary>
        ///     Property IsStarted
        /// </summary>
        public Boolean IsStarted
            => Next.IsStarted;

        /// <summary>
        ///     Method Start
        /// </summary>
        public virtual void Start()
        {
            if ( Command == null )
                throw new InvalidOperationException( "command cannot be null when Start is called." );

            if ( Exception == null )
                throw new InvalidOperationException( "exception cannot be null when Start is called." );

            Next.Start();
        }

        public virtual void Stop()
            => Next.Stop();

        /// <summary>
        ///     Method AsyncRequest
        /// </summary>
        /// <returns>A FutureResponse</returns>
        /// <param name="command">A  Command</param>
        public virtual FutureResponse AsyncRequest( ICommand command )
            => Next.AsyncRequest( command );

        /// <summary>
        ///     Gets or sets the timeout for sending asynchronous messages or commands.
        /// </summary>
        public TimeSpan AsyncTimeout
        {
            get => Next.AsyncTimeout;
            set => Next.AsyncTimeout = value;
        }

        public Boolean IsConnected
            => Next.IsConnected;

        public Boolean IsFaultTolerant
            => Next.IsFaultTolerant;

        public Object Narrow( Type type )
            => GetType() == type ? this : Next?.Narrow( type );

        /// <summary>
        ///     Method Oneway
        /// </summary>
        /// <param name="command">A  Command</param>
        public virtual void Oneway( ICommand command ) => Next.Oneway( command );

        public Uri RemoteAddress => Next.RemoteAddress;

        /// <summary>
        ///     Method Request with time out for Response.
        /// </summary>
        /// <returns>A Response</returns>
        /// <param name="command">A  Command</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public virtual Response Request( ICommand command, TimeSpan timeout )
            => Next.Request( command, timeout );

        /// <summary>
        ///     Gets or sets the timeout for sending synchronous messages or commands.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Next.Timeout;
            set => Next.Timeout = value;
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