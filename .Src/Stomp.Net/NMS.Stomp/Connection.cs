#region Usings

using System;
using System.Collections.Concurrent;
using System.Threading;
using Extend;
using JetBrains.Annotations;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Stomp.Threads;
using Stomp.Net.Stomp.Transport;
using Stomp.Net.Stomp.Util;
using Stomp.Net.Util;
using Stomp.Net.Utilities;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Represents a connection with a message broker
    /// </summary>
    public class Connection : Disposable, IConnection
    {
        #region Constants

        private static readonly IdGenerator ConnectionIdGenerator = new IdGenerator();

        #endregion

        #region Fields

        private readonly IdGenerator _clientIdGenerator;
        private readonly Atomic<Boolean> _closed = new Atomic<Boolean>( false );
        private readonly Atomic<Boolean> _closing = new Atomic<Boolean>( false );
        private readonly Atomic<Boolean> _connected = new Atomic<Boolean>( false );
        private readonly Object _connectedLock = new Object();
        private readonly ConcurrentDictionary<ConsumerId, IDispatcher> _dispatchers = new ConcurrentDictionary<ConsumerId, IDispatcher>();
        private readonly ThreadPoolExecutor _executor = new ThreadPoolExecutor();
        private readonly ConnectionInfo _info;
        private readonly Object _myLock = new Object();
        private readonly ConcurrentDictionary<Session, Session> _sessions = new ConcurrentDictionary<Session, Session>();
        private readonly Atomic<Boolean> _started = new Atomic<Boolean>( false );

        /// <summary>
        ///     The STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        private readonly ITransportFactory _transportFactory;
        private readonly Atomic<Boolean> _transportFailed = new Atomic<Boolean>( false );
        private Int32 _localTransactionCounter;
        private Int32 _sessionCounter;
        private Int32 _temporaryDestinationCounter;
        private CountDownLatch _transportInterruptionProcessingComplete;
        private Boolean _userSpecifiedClientId;

        #endregion

        #region Properties

        private Uri BrokerUri { get; }

        private ITransport Transport { get; set; }

        public Exception FirstFailureError { get; private set; }

        /// <summary>
        ///     The Default Client Id used if the ClientId property is not set explicit.
        /// </summary>
        public String DefaultClientId
        {
            set
            {
                _info.ClientId = value;
                _userSpecifiedClientId = true;
            }
        }

        public ConnectionId ConnectionId => _info.ConnectionId;

        public PrefetchPolicy PrefetchPolicy { get; set; } = new PrefetchPolicy();

        internal MessageTransformation MessageTransformation { get; }

        #endregion

        #region Ctor

        public Connection( Uri connectionUri, ITransport transport, IdGenerator clientIdGenerator, [NotNull] StompConnectionSettings stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof(stompConnectionSettings) );

            _stompConnectionSettings = stompConnectionSettings;
            _transportFactory = new TransportFactory( _stompConnectionSettings );
            BrokerUri = connectionUri;
            _clientIdGenerator = clientIdGenerator;

            SetTransport( transport );

            _info = new ConnectionInfo
            {
                ConnectionId = new ConnectionId { Value = ConnectionIdGenerator.GenerateId() },
                Host = BrokerUri.Host,
                UserName = _stompConnectionSettings.UserName,
                Password = _stompConnectionSettings.Password
            };

            MessageTransformation = new StompMessageTransformation( this );
        }

        #endregion

        public String ClientId
        {
            get => _info.ClientId;
            set
            {
                if ( _connected.Value )
                    throw new StompException( "You cannot change the ClientId once the Connection is connected" );

                _info.ClientId = value;
                _userSpecifiedClientId = true;
                CheckConnected();
            }
        }

        public void Close()
        {
            lock ( _myLock )
            {
                if ( _closed.Value )
                    return;

                try
                {
                    Tracer.Info( "Closing Connection." );
                    _closing.Value = true;

                    foreach ( var session in _sessions )
                        session.Value.DoClose();
                    _sessions.Clear();

                    if ( _connected.Value )
                    {
                        var shutdowninfo = new ShutdownInfo();
                        Transport.Oneway( shutdowninfo );
                    }

                    Tracer.Info( "Disposing of the Transport." );
                    Transport.Stop();
                    Transport.Dispose();
                }
                catch ( Exception ex )
                {
                    Tracer.Error( $"Error during connection close: {ex}" );
                }
                finally
                {
                    Transport = null;
                    _closed.Value = true;
                    _connected.Value = false;
                    _closing.Value = false;
                }
            }
        }

        /// <summary>
        ///     An asynchronous listener that is notified when a Fault tolerant connection
        ///     has been interrupted.
        /// </summary>
        public event ConnectionInterruptedListener ConnectionInterruptedListener;

        /// <summary>
        ///     An asynchronous listener that is notified when a Fault tolerant connection
        ///     has been resumed.
        /// </summary>
        public event ConnectionResumedListener ConnectionResumedListener;

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession()
            => CreateSession( _stompConnectionSettings.AcknowledgementMode );

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession( AcknowledgementMode sessionAcknowledgementMode )
        {
            var info = CreateSessionInfo();
            var session = new Session( this, info, sessionAcknowledgementMode, _stompConnectionSettings );

            if ( IsStarted )
                session.Start();

            if ( !_sessions.TryAdd( session, session ) )
                Tracer.Warn( $"Failed to add session with id: '{session.SessionId}'." );

            return session;
        }

        /// <summary>
        ///     A delegate that can receive transport level exceptions.
        /// </summary>
        public event Action<Exception> ExceptionListener;

        public void PurgeTempDestinations()
        {
        }

        /// <summary>
        ///     Get/or set the redelivery policy for this connection.
        /// </summary>
        public IRedeliveryPolicy RedeliveryPolicy { get; set; }

        /// <summary>
        ///     This property determines if the asynchronous message delivery of incoming
        ///     messages has been started for this connection.
        /// </summary>
        public Boolean IsStarted
            => _started.Value;

        /// <summary>
        ///     Starts asynchronous message delivery of incoming messages for this connection.
        ///     Synchronous delivery is unaffected.
        /// </summary>
        public void Start()
        {
            CheckConnected();
            if ( !_started.CompareAndSet( false, true ) )
                return;

            foreach ( var session in _sessions )
                session.Value.Start();
        }

        /// <summary>
        ///     Temporarily stop asynchronous delivery of inbound messages for this connection.
        ///     The sending of outbound messages is unaffected.
        /// </summary>
        public void Stop()
        {
            CheckConnected();
            if ( !_started.CompareAndSet( true, false ) )
                return;

            foreach ( var session in _sessions )
                session.Value.Stop();
        }

        /// <summary>
        ///     Creates a new local transaction ID
        /// </summary>
        public TransactionId CreateLocalTransactionId()
            => new TransactionId( Interlocked.Increment( ref _localTransactionCounter ), ConnectionId );

        /// <summary>
        ///     Creates a new temporary destination name
        /// </summary>
        public String CreateTemporaryDestinationName() 
            => _info.ConnectionId.Value + ":" + Interlocked.Increment( ref _temporaryDestinationCounter );

        public void Oneway( ICommand command )
        {
            CheckConnected();

            try
            {
                Transport.Oneway( command );
            }
            catch ( Exception ex )
            {
                throw ex.Create();
            }
        }

        /// <summary>
        ///     Performs a synchronous request-response with the broker
        /// </summary>
        public Response SyncRequest( ICommand command ) 
            => SyncRequest( command, _stompConnectionSettings.RequestTimeout );

        public Response SyncRequest( ICommand command, TimeSpan requestTimeout )
        {
            CheckConnected();

            try
            {
                var response = Transport.Request( command, requestTimeout );
                if ( !( response is ExceptionResponse ) )
                    return response;

                var exceptionResponse = (ExceptionResponse) response;
                var brokerError = exceptionResponse.Exception;
                throw new BrokerException( brokerError );
            }
            catch ( Exception ex )
            {
                throw ex.Create();
            }
        }

        /// <summary>
        ///     Method invoked when the instance gets disposed.
        /// </summary>
        protected override void Disposed()
        {
            try
            {
                // For now we do not distinguish between Dispose() and Close().
                // In theory Dispose should possibly be lighter-weight and perform a (faster)
                // disorderly close.
                Close();
            }
            catch
            {
                // Ignore network errors.
            }
        }

        internal void AddDispatcher( ConsumerId id, IDispatcher dispatcher )
        {
            if ( !_dispatchers.TryAdd( id, dispatcher ) )
                Tracer.Warn( $"Failed to add dispatcher with id '{id}'." );
        }

        internal void OnSessionException( Session sender, Exception exception )
        {
            if ( ExceptionListener == null )
                return;
            try
            {
                ExceptionListener( exception );
            }
            catch
            {
                sender.Close();
            }
        }

        internal void RemoveDispatcher( ConsumerId id )
        {
            if ( !_dispatchers.TryRemove( id, out IDispatcher _ ) )
                Tracer.Warn( $"Failed to remove dispatcher with id '{id}'." );
        }

        internal void RemoveSession( Session session )
        {
            if ( _closing.Value )
                return;

            if ( !_sessions.TryRemove( session, out Session _ ) )
                Tracer.Warn( $"Failed to remove session with session id: '{session.SessionId}'." );
        }

        internal void TransportInterruptionProcessingComplete()
        {
            var cdl = _transportInterruptionProcessingComplete;
            cdl?.CountDown();
        }

        private void AsyncCallExceptionListener( Object error )
        {
            var exception = error as StompException;
            ExceptionListener?.Invoke( exception );
        }

        private void AsyncOnExceptionHandler( Object error )
        {
            var cause = error as Exception;

            MarkTransportFailed( cause );

            try
            {
                Transport.Dispose();
            }
            catch ( Exception ex )
            {
                Tracer.Warn( $"Caught Exception While disposing of Transport: {ex}." );
            }

            foreach ( var session in _sessions )
                try
                {
                    session.Value.Dispose();
                }
                catch ( Exception ex )
                {
                    Tracer.Warn( $"Caught Exception While disposing of Sessions: {ex}." );
                }
        }

        /// <summary>
        ///     Check and ensure that the connection objcet is connected.  If it is not
        ///     connected or is closed, a ConnectionClosedException is thrown.
        /// </summary>
        private void CheckConnected()
        {
            if ( _closed.Value )
                throw new ConnectionClosedException();

            if ( _connected.Value )
                return;
            var timeoutTime = DateTime.Now + _stompConnectionSettings.RequestTimeout;
            var waitCount = 1;

            while ( true )
            {
                if ( Monitor.TryEnter( _connectedLock ) )
                    try
                    {
                        if ( _closed.Value || _closing.Value )
                            break;
                        else if ( !_connected.Value )
                        {
                            if ( !_userSpecifiedClientId )
                                _info.ClientId = _clientIdGenerator.GenerateId();

                            try
                            {
                                if ( null != Transport )
                                {
                                    // Make sure the transport is started.
                                    if ( !Transport.IsStarted )
                                        Transport.Start();

                                    // Send the connection and see if an ack/nak is returned.
                                    var response = Transport.Request( _info, _stompConnectionSettings.RequestTimeout );
                                    if ( !( response is ExceptionResponse ) )
                                        _connected.Value = true;
                                    else
                                    {
                                        var error = response as ExceptionResponse;
                                        var exception = CreateExceptionFromBrokerError( error.Exception );
                                        // This is non-recoverable.
                                        // Shutdown the transport connection, and re-create it, but don't start it.
                                        // It will be started if the connection is re-attempted.
                                        Transport.Stop();
                                        var newTransport = _transportFactory.CreateTransport( BrokerUri );
                                        SetTransport( newTransport );
                                        throw exception;
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                Tracer.Error( ex.ToString() );
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit( _connectedLock );
                    }

                if ( _connected.Value || _closed.Value || _closing.Value || DateTime.Now > timeoutTime )
                    break;

                // Back off from being overly aggressive.  Having too many threads
                // aggressively trying to connect to a down broker pegs the CPU.
                Thread.Sleep( 5 * waitCount++ );
            }

            if ( !_connected.Value )
                throw new ConnectionClosedException();
        }

        private static StompException CreateExceptionFromBrokerError( BrokerError brokerError )
        {
            var exceptionClassName = brokerError.ExceptionClass;

            if ( exceptionClassName.IsEmpty() )
                return new BrokerException( brokerError );

            return new InvalidClientIdException( brokerError.Message );
        }

        private SessionInfo CreateSessionInfo()
        {
            var answer = new SessionInfo();
            var sessionId = new SessionId( Interlocked.Increment( ref _sessionCounter ), ConnectionId );
            answer.SessionId = sessionId;
            return answer;
        }

        private void DispatchMessage( MessageDispatch dispatch )
        {
            if ( _dispatchers.TryGetValue( dispatch.ConsumerId, out var dispatcher ) )
            {
                // Can be null when a consumer has sent a MessagePull and there was
                // no available message at the broker to dispatch.
                if ( dispatch.Message != null )
                {
                    dispatch.Message.ReadOnlyBody = true;
                    dispatch.Message.RedeliveryCounter = dispatch.RedeliveryCounter;
                }

                dispatcher.Dispatch( dispatch );

                return;
            }

            Tracer.Error( $"No such consumer active: {dispatch.ConsumerId}." );
        }

        private void MarkTransportFailed( Exception error )
        {
            _transportFailed.Value = true;
            if ( FirstFailureError == null )
                FirstFailureError = error;
        }

        private void OnAsyncException( Exception error )
        {
            if ( _closed.Value || _closing.Value )
                return;
            if ( ExceptionListener != null )
            {
                if ( !( error is StompException ) )
                    error = error.Create();
                var e = (StompException) error;

                // Called in another thread so that processing can continue
                // here, ensures no lock contention.
                _executor.QueueUserWorkItem( AsyncCallExceptionListener, e );
            }
            else
                Tracer.Warn( $"Async exception with no exception listener: {error}" );
        }

        /// <summary>
        ///     Handle incoming commands
        /// </summary>
        /// <param name="commandTransport">An ITransport</param>
        /// <param name="command">A  Command</param>
        private void OnCommand( ITransport commandTransport, ICommand command )
        {
            if ( command.IsMessageDispatch )
            {
                // We wait if the Connection is still processing interruption
                // code to reset the MessageConsumers.
                WaitForTransportInterruptionProcessingToComplete();
                DispatchMessage( (MessageDispatch) command );
            }
            else if ( command.IsWireFormatInfo )
            {
                // Ignore for now, might need to save if off later.
            }
            else if ( command.IsKeepAliveInfo )
            {
                // Ignore only the InactivityMonitor cares about this one.
            }
            else if ( command.IsErrorCommand )
            {
                if ( _closing.Value || _closed.Value )
                    return;
                var connectionError = (ConnectionError) command;
                var brokerError = connectionError.Exception;
                var message = "Broker connection error.";
                var cause = "";

                if ( null != brokerError )
                {
                    message = brokerError.Message;
                    if ( null != brokerError.Cause )
                        cause = brokerError.Cause.Message;
                }

                OnException( new StompConnectionException( message, cause ) );
            }
            else
                Tracer.Error( $"Unknown command: {command}" );
        }

        private void OnException( Exception error )
        {
            // Will fire an exception listener callback if there's any set.
            OnAsyncException( error );

            if ( !_closing.Value && !_closed.Value )
                _executor.QueueUserWorkItem( AsyncOnExceptionHandler, error );
        }

        private void OnTransportException( ITransport sender, Exception exception ) => OnException( exception );

        private void OnTransportInterrupted( ITransport sender )
        {
            _transportInterruptionProcessingComplete = new CountDownLatch( _dispatchers.Count );
            Tracer.Warn( $"Transport interrupted, dispatchers: '{_dispatchers.Count}'" );

            foreach ( var session in _sessions )
                session.Value.ClearMessagesInProgress();

            if ( ConnectionInterruptedListener == null || _closing.Value )
                return;
            try
            {
                ConnectionInterruptedListener();
            }
            catch
            {
                // ignored
            }
        }

        private void OnTransportResumed( ITransport sender )
        {
            if ( ConnectionResumedListener == null || _closing.Value )
                return;

            try
            {
                ConnectionResumedListener();
            }
            catch
            {
                // ignored
            }
        }

        private void SetTransport( ITransport newTransport )
        {
            Transport = newTransport;
            Transport.Command = OnCommand;
            Transport.Exception = OnTransportException;
            Transport.Interrupted = OnTransportInterrupted;
            Transport.Resumed = OnTransportResumed;
        }

        private void WaitForTransportInterruptionProcessingToComplete()
        {
            var cdl = _transportInterruptionProcessingComplete;
            if ( cdl == null )
                return;

            if ( _closed.Value || cdl.Remaining <= 0 )
                return;

            Tracer.Warn( $"Dispatch paused, waiting for outstanding dispatch interruption processing ({cdl.Remaining}) to complete.." );
            cdl.AwaitOperation( 10.ToSeconds() );
        }
    }
}