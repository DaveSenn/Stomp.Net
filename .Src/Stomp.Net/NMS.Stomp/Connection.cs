#region Usings

using System;
using System.Collections;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.Threads;
using Apache.NMS.Stomp.Transport;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;
using Extend;
using JetBrains.Annotations;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Represents a connection with a message broker
    /// </summary>
    public class Connection : IConnection
    {
        #region Constants

        private static readonly IdGenerator CONNECTION_ID_GENERATOR = new IdGenerator();

        #endregion

        #region Fields

        /// <summary>
        ///     The STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        private readonly IdGenerator clientIdGenerator;
        private readonly Atomic<Boolean> closed = new Atomic<Boolean>( false );
        private readonly Atomic<Boolean> closing = new Atomic<Boolean>( false );
        private readonly Atomic<Boolean> connected = new Atomic<Boolean>( false );

        private readonly Object connectedLock = new Object();
        private readonly IDictionary dispatchers = Hashtable.Synchronized( new Hashtable() );
        private readonly ThreadPoolExecutor executor = new ThreadPoolExecutor();
        private readonly ConnectionInfo info;
        private readonly Object myLock = new Object();
        private readonly IList sessions = ArrayList.Synchronized( new ArrayList() );
        private readonly Atomic<Boolean> started = new Atomic<Boolean>( false );
        private readonly Atomic<Boolean> transportFailed = new Atomic<Boolean>( false );

        private Boolean disposed;
        private Int32 localTransactionCounter;
        private ConnectionMetaData metaData;

        private Int32 sessionCounter;
        private Int32 temporaryDestinationCounter;
        private CountDownLatch transportInterruptionProcessingComplete;

        private Boolean userSpecifiedClientID;

        #endregion

        #region Properties

        public String UserName
        {
            get { return info.UserName; }
            set { info.UserName = value; }
        }

        public String Password
        {
            get { return info.Password; }
            set { info.Password = value; }
        }

        /// <summary>
        ///     This property indicates whether or not async send is enabled.
        /// </summary>
        public Boolean AsyncSend { get; set; } = false;

        /// <summary>
        ///     This property sets the acknowledgment mode for the connection.
        ///     The URI parameter connection.ackmode can be set to a string value
        ///     that maps to the enumeration value.
        /// </summary>
        public String AckMode
        {
            set { AcknowledgementMode = NMSConvert.ToAcknowledgementMode( value ); }
        }

        /// <summary>
        ///     This property forces all messages that are sent to be sent synchronously overriding
        ///     any usage of the AsyncSend flag. This can reduce performance in some cases since the
        ///     only messages we normally send synchronously are Persistent messages not sent in a
        ///     transaction. This options guarantees that no send will return until the broker has
        ///     acknowledge receipt of the message
        /// </summary>
        public Boolean AlwaysSyncSend { get; set; } = false;

        /// <summary>
        ///     This property indicates whether Message's should be copied before being sent via
        ///     one of the Connection's send methods.  Copying the Message object allows the user
        ///     to resuse the Object over for another send.  If the message isn't copied performance
        ///     can improve but the user must not reuse the Object as it may not have been sent
        ///     before they reset its payload.
        /// </summary>
        public Boolean CopyMessageOnSend { get; set; } = true;

        /// <summary>
        ///     This property indicates whether or not async sends are used for
        ///     message acknowledgement messages.  Sending Acks async can improve
        ///     performance but may decrease reliability.
        /// </summary>
        public Boolean SendAcksAsync { get; set; } = false;

        /// <summary>
        ///     synchronously or asynchronously by the broker.  Set to false for a slow
        ///     consumer and true for a fast consumer.
        /// </summary>
        public Boolean DispatchAsync { get; set; } = true;

        /// <summary>
        ///     Sets the default Transformation attribute applied to Consumers.  If a consumer
        ///     is to receive Map messages from the Broker then the user should set the "jms-map-xml"
        ///     transformation on the consumer so that all MapMessages are sent as XML.
        /// </summary>
        public String Transformation { get; set; } = null;

        public Uri BrokerUri { get; }

        public ITransport ITransport { get; set; }

        public Boolean TransportFailed
        {
            get { return transportFailed.Value; }
        }

        public Exception FirstFailureError { get; private set; }

        /// <summary>
        ///     The Default Client Id used if the ClientId property is not set explicity.
        /// </summary>
        public String DefaultClientId
        {
            set
            {
                info.ClientId = value;
                userSpecifiedClientID = true;
            }
        }

        public ConnectionId ConnectionId
        {
            get { return info.ConnectionId; }
        }

        public PrefetchPolicy PrefetchPolicy { get; set; } = new PrefetchPolicy();

        internal MessageTransformation MessageTransformation { get; }

        #endregion

        #region Ctor

        public Connection( Uri connectionUri, ITransport transport, IdGenerator clientIdGenerator, [NotNull] StompConnectionSettings stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
            BrokerUri = connectionUri;
            this.clientIdGenerator = clientIdGenerator;

            SetTransport( transport );

            var id = new ConnectionId { Value = CONNECTION_ID_GENERATOR.GenerateId() };

            info = new ConnectionInfo
            {
                ConnectionId = id,
                Host = BrokerUri.Host
            };

            MessageTransformation = new StompMessageTransformation( this );
        }

        #endregion

        public AcknowledgementMode AcknowledgementMode { get; set; } = AcknowledgementMode.AutoAcknowledge;

        public String ClientId
        {
            get { return info.ClientId; }
            set
            {
                if ( connected.Value )
                    throw new NMSException( "You cannot change the ClientId once the Connection is connected" );

                info.ClientId = value;
                userSpecifiedClientID = true;
                CheckConnected();
            }
        }

        public void Close()
        {
            lock ( myLock )
            {
                if ( closed.Value )
                    return;

                try
                {
                    Tracer.Info( "Closing Connection." );
                    closing.Value = true;
                    lock ( sessions.SyncRoot )
                        foreach ( Session session in sessions )
                            session.DoClose();
                    sessions.Clear();

                    if ( connected.Value )
                    {
                        var shutdowninfo = new ShutdownInfo();
                        ITransport.Oneway( shutdowninfo );
                    }

                    Tracer.Info( "Disposing of the Transport." );
                    ITransport.Stop();
                    ITransport.Dispose();
                }
                catch ( Exception ex )
                {
                    Tracer.ErrorFormat( "Error during connection close: {0}", ex.Message );
                }
                finally
                {
                    ITransport = null;
                    closed.Value = true;
                    connected.Value = false;
                    closing.Value = false;
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

        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession() => CreateSession( AcknowledgementMode );

        /// <summary>
        ///     Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession( AcknowledgementMode sessionAcknowledgementMode )
        {
            var info = CreateSessionInfo( sessionAcknowledgementMode );
            var session = new Session( this, info, sessionAcknowledgementMode, DispatchAsync );

            // Set properties on session using parameters prefixed with "session."
            if ( !String.IsNullOrEmpty( BrokerUri.Query ) && !BrokerUri.OriginalString.EndsWith( ")" ) )
            {
                // Since the Uri class will return the end of a Query string found in a Composite
                // URI we must ensure that we trim that off before we proceed.
                var query = BrokerUri.Query.Substring( BrokerUri.Query.LastIndexOf( ")" ) + 1 );
                var options = URISupport.ParseQuery( query );
                options = URISupport.GetProperties( options, "session." );
                URISupport.SetProperties( session, options );
            }

            session.ConsumerTransformer = ConsumerTransformer;
            session.ProducerTransformer = ProducerTransformer;

            if ( IsStarted )
                session.Start();

            sessions.Add( session );
            return session;
        }

        /// <summary>
        ///     A delegate that can receive transport level exceptions.
        /// </summary>
        public event ExceptionListener ExceptionListener;

        public IConnectionMetaData MetaData
        {
            get { return metaData ?? ( metaData = new ConnectionMetaData() ); }
        }

        public ProducerTransformerDelegate ProducerTransformer { get; set; }

        public void PurgeTempDestinations()
        {
        }

        /// <summary>
        ///     Get/or set the redelivery policy for this connection.
        /// </summary>
        public IRedeliveryPolicy RedeliveryPolicy { get; set; }

        public TimeSpan RequestTimeout { get; set; }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        ///     This property determines if the asynchronous message delivery of incoming
        ///     messages has been started for this connection.
        /// </summary>
        public Boolean IsStarted
        {
            get { return started.Value; }
        }

        /// <summary>
        ///     Starts asynchronous message delivery of incoming messages for this connection.
        ///     Synchronous delivery is unaffected.
        /// </summary>
        public void Start()
        {
            CheckConnected();
            if ( started.CompareAndSet( false, true ) )
                lock ( sessions.SyncRoot )
                    foreach ( Session session in sessions )
                        session.Start();
        }

        /// <summary>
        ///     Temporarily stop asynchronous delivery of inbound messages for this connection.
        ///     The sending of outbound messages is unaffected.
        /// </summary>
        public void Stop()
        {
            CheckConnected();
            if ( started.CompareAndSet( true, false ) )
                lock ( sessions.SyncRoot )
                    foreach ( Session session in sessions )
                        session.Stop();
        }

        /// <summary>
        ///     Creates a new local transaction ID
        /// </summary>
        public TransactionId CreateLocalTransactionId()
        {
            var id = new TransactionId();
            id.ConnectionId = ConnectionId;
            id.Value = Interlocked.Increment( ref localTransactionCounter );
            return id;
        }

        /// <summary>
        ///     Creates a new temporary destination name
        /// </summary>
        public String CreateTemporaryDestinationName() => info.ConnectionId.Value + ":" + Interlocked.Increment( ref temporaryDestinationCounter );

        public void Oneway( Command command )
        {
            CheckConnected();

            try
            {
                ITransport.Oneway( command );
            }
            catch ( Exception ex )
            {
                throw NMSExceptionSupport.Create( ex );
            }
        }

        // Implementation methods

        /// <summary>
        ///     Performs a synchronous request-response with the broker
        /// </summary>
        public Response SyncRequest( Command command ) => SyncRequest( command, RequestTimeout );

        public Response SyncRequest( Command command, TimeSpan requestTimeout )
        {
            CheckConnected();

            try
            {
                var response = ITransport.Request( command, requestTimeout );
                if ( response is ExceptionResponse )
                {
                    var exceptionResponse = (ExceptionResponse) response;
                    var brokerError = exceptionResponse.Exception;
                    throw new BrokerException( brokerError );
                }
                return response;
            }
            catch ( Exception ex )
            {
                throw NMSExceptionSupport.Create( ex );
            }
        }

        protected SessionInfo CreateSessionInfo( AcknowledgementMode sessionAcknowledgementMode )
        {
            var answer = new SessionInfo();
            var sessionId = new SessionId();
            sessionId.ConnectionId = info.ConnectionId.Value;
            sessionId.Value = Interlocked.Increment( ref sessionCounter );
            answer.SessionId = sessionId;
            return answer;
        }

        protected void DispatchMessage( MessageDispatch dispatch )
        {
            lock ( dispatchers.SyncRoot )
                if ( dispatchers.Contains( dispatch.ConsumerId ) )
                {
                    var dispatcher = (IDispatcher) dispatchers[dispatch.ConsumerId];

                    // Can be null when a consumer has sent a MessagePull and there was
                    // no available message at the broker to dispatch.
                    if ( dispatch.Message != null )
                    {
                        dispatch.Message.ReadOnlyBody = true;
                        dispatch.Message.ReadOnlyProperties = true;
                        dispatch.Message.RedeliveryCounter = dispatch.RedeliveryCounter;
                    }

                    dispatcher.Dispatch( dispatch );

                    return;
                }

            Tracer.ErrorFormat( "No such consumer active: {0}.", dispatch.ConsumerId );
        }

        protected void Dispose( Boolean disposing )
        {
            if ( disposed )
                return;

            if ( disposing )
            {
                // Dispose managed code here.
            }

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

            disposed = true;
        }

        /// <summary>
        ///     Handle incoming commands
        /// </summary>
        /// <param name="commandTransport">An ITransport</param>
        /// <param name="command">A  Command</param>
        protected void OnCommand( ITransport commandTransport, Command command )
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
                if ( !closing.Value && !closed.Value )
                {
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

                    OnException( new NMSConnectionException( message, cause ) );
                }
            }
            else
            {
                Tracer.ErrorFormat( "Unknown command: {0}", command );
            }
        }

        protected void OnTransportException( ITransport sender, Exception exception ) => OnException( exception );

        protected void OnTransportInterrupted( ITransport sender )
        {
            Tracer.Debug( "Transport has been Interrupted." );

            transportInterruptionProcessingComplete = new CountDownLatch( dispatchers.Count );
            if ( Tracer.IsDebugEnabled )
                Tracer.DebugFormat( "transport interrupted, dispatchers: {0}", dispatchers.Count );

            foreach ( Session session in sessions )
                session.ClearMessagesInProgress();

            if ( ConnectionInterruptedListener != null && !closing.Value )
                try
                {
                    ConnectionInterruptedListener();
                }
                catch
                {
                }
        }

        protected void OnTransportResumed( ITransport sender )
        {
            Tracer.Debug( "Transport has resumed normal operation." );

            if ( ConnectionResumedListener != null && !closing.Value )
                try
                {
                    ConnectionResumedListener();
                }
                catch
                {
                }
        }

        internal void addDispatcher( ConsumerId id, IDispatcher dispatcher ) => dispatchers.Add( id, dispatcher );

        /// <summary>
        ///     Check and ensure that the connection objcet is connected.  If it is not
        ///     connected or is closed, a ConnectionClosedException is thrown.
        /// </summary>
        internal void CheckConnected()
        {
            if ( closed.Value )
                throw new ConnectionClosedException();

            if ( !connected.Value )
            {
                var timeoutTime = DateTime.Now + RequestTimeout;
                var waitCount = 1;

                while ( true )
                {
                    if ( Monitor.TryEnter( connectedLock ) )
                        try
                        {
                            if ( closed.Value || closing.Value )
                            {
                                break;
                            }
                            else if ( !connected.Value )
                            {
                                if ( !userSpecifiedClientID )
                                    info.ClientId = clientIdGenerator.GenerateId();

                                try
                                {
                                    if ( null != ITransport )
                                    {
                                        // Make sure the transport is started.
                                        if ( !ITransport.IsStarted )
                                            ITransport.Start();

                                        // Send the connection and see if an ack/nak is returned.
                                        var response = ITransport.Request( info, RequestTimeout );
                                        if ( !( response is ExceptionResponse ) )
                                        {
                                            connected.Value = true;
                                        }
                                        else
                                        {
                                            var error = response as ExceptionResponse;
                                            var exception = CreateExceptionFromBrokerError( error.Exception );
                                            // This is non-recoverable.
                                            // Shutdown the transport connection, and re-create it, but don't start it.
                                            // It will be started if the connection is re-attempted.
                                            ITransport.Stop();
                                            var newTransport = TransportFactory.CreateTransport( BrokerUri, _stompConnectionSettings );
                                            SetTransport( newTransport );
                                            throw exception;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit( connectedLock );
                        }

                    if ( connected.Value || closed.Value || closing.Value || DateTime.Now > timeoutTime )
                        break;

                    // Back off from being overly aggressive.  Having too many threads
                    // aggressively trying to connect to a down broker pegs the CPU.
                    Thread.Sleep( 5 * waitCount++ );
                }

                if ( !connected.Value )
                    throw new ConnectionClosedException();
            }
        }

        internal void OnAsyncException( Exception error )
        {
            if ( !closed.Value && !closing.Value )
                if ( ExceptionListener != null )
                {
                    if ( !( error is NMSException ) )
                        error = NMSExceptionSupport.Create( error );
                    var e = (NMSException) error;

                    // Called in another thread so that processing can continue
                    // here, ensures no lock contention.
                    executor.QueueUserWorkItem( AsyncCallExceptionListener, e );
                }
                else
                {
                    Tracer.DebugFormat( "Async exception with no exception listener: {0}", error.Message );
                }
        }

        internal void OnException( Exception error )
        {
            // Will fire an exception listener callback if there's any set.
            OnAsyncException( error );

            if ( !closing.Value && !closed.Value )
                executor.QueueUserWorkItem( AsyncOnExceptionHandler, error );
        }

        internal void OnSessionException( Session sender, Exception exception )
        {
            if ( ExceptionListener != null )
                try
                {
                    ExceptionListener( exception );
                }
                catch
                {
                    sender.Close();
                }
        }

        internal void removeDispatcher( ConsumerId id ) => dispatchers.Remove( id );

        internal void RemoveSession( Session session )
        {
            if ( !closing.Value )
                sessions.Remove( session );
        }

        internal void TransportInterruptionProcessingComplete()
        {
            var cdl = transportInterruptionProcessingComplete;
            if ( cdl != null )
                cdl.countDown();
        }

        private void AsyncCallExceptionListener( Object error )
        {
            var exception = error as NMSException;
            ExceptionListener( exception );
        }

        private void AsyncOnExceptionHandler( Object error )
        {
            var cause = error as Exception;

            MarkTransportFailed( cause );

            try
            {
                ITransport.Dispose();
            }
            catch ( Exception ex )
            {
                Tracer.DebugFormat( "Caught Exception While disposing of Transport: {0}", ex.Message );
            }

            IList sessionsCopy = null;
            lock ( sessions.SyncRoot )
                sessionsCopy = new ArrayList( sessions );

            // Use a copy so we don't concurrently modify the Sessions list if the
            // client is closing at the same time.
            foreach ( Session session in sessionsCopy )
                try
                {
                    session.Dispose();
                }
                catch ( Exception ex )
                {
                    Tracer.DebugFormat( "Caught Exception While disposing of Sessions: {0}", ex.Message );
                }
        }

        private NMSException CreateExceptionFromBrokerError( BrokerError brokerError )
        {
            var exceptionClassName = brokerError.ExceptionClass;

            if ( String.IsNullOrEmpty( exceptionClassName ) )
                return new BrokerException( brokerError );

            return new InvalidClientIDException( brokerError.Message );
        }

        private void MarkTransportFailed( Exception error )
        {
            transportFailed.Value = true;
            if ( FirstFailureError == null )
                FirstFailureError = error;
        }

        private void SetTransport( ITransport newTransport )
        {
            ITransport = newTransport;
            ITransport.Command = OnCommand;
            ITransport.Exception = OnTransportException;
            ITransport.Interrupted = OnTransportInterrupted;
            ITransport.Resumed = OnTransportResumed;
        }

        private void WaitForTransportInterruptionProcessingToComplete()
        {
            var cdl = transportInterruptionProcessingComplete;
            if ( cdl != null )
                if ( !closed.Value && cdl.Remaining > 0 )
                {
                    Tracer.WarnFormat( "dispatch paused, waiting for outstanding dispatch interruption " +
                                       "processing ({0}) to complete..",
                                       cdl.Remaining );
                    cdl.await( TimeSpan.FromSeconds( 10 ) );
                }
        }

        ~Connection()
        {
            Dispose( false );
        }
    }
}