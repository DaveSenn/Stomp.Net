#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.State;
using Apache.NMS.Stomp.Threads;
using Apache.NMS.Util;
using Extend;
using JetBrains.Annotations;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport.Failover
{
    /// <summary>
    ///     A Transport that is made reliable by being able to fail over to another
    ///     transport when a transport failure is detected.
    /// </summary>
    public class FailoverTransport : ICompositeTransport, IComparable
    {
        #region Constants

        private static Int32 _idCounter;

        #endregion

        #region Fields

        /// <summary>
        ///     The STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        /// <summary>
        ///     Stores the transport factory.
        /// </summary>
        private readonly ITransportFactory _transportFactory;

        private readonly AtomicReference<ITransport> connectedTransport = new AtomicReference<ITransport>( null );
        private readonly Int32 id;
        private readonly Object mutex = new Object();

        private readonly Mutex reconnectMutex = new Mutex();
        private readonly Dictionary<Int32, ICommand> requestMap = new Dictionary<Int32, ICommand>();
        private readonly Mutex sleepMutex = new Mutex();
        private readonly ConnectionStateTracker stateTracker = new ConnectionStateTracker();
        private readonly List<Uri> uris = new List<Uri>();

        private Int32 connectFailures;
        private Exception connectionFailure;

        private Uri failedConnectTransportURI;
        private volatile Exception failure;
        private Boolean firstConnection = true;
        private TaskRunner reconnectTask;

        #endregion

        #region Properties

        private List<Uri> ConnectList
        {
            get
            {
                var l = new List<Uri>( uris );
                var removed = false;
                if ( failedConnectTransportURI != null )
                    removed = l.Remove( failedConnectTransportURI );

                if ( Randomize )
                {
                    // Randomly, reorder the list by random swapping
                    var r = new Random( DateTime.Now.Millisecond );
                    for ( var i = 0; i < l.Count; i++ )
                    {
                        var p = r.Next( l.Count );
                        var t = l[p];
                        l[p] = l[i];
                        l[i] = t;
                    }
                }

                if ( removed )
                    l.Add( failedConnectTransportURI );

                return l;
            }
        }

        public Boolean IsDisposed { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="FailoverTransport" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">stompConnectionSettings can not be null.</exception>
        /// <param name="stompConnectionSettings">Some STOMP settings.</param>
        public FailoverTransport( [NotNull] StompConnectionSettings stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
            _transportFactory = new TransportFactory( _stompConnectionSettings );
            id = _idCounter++;
        }

        #endregion

        public Int32 CompareTo( Object o )
        {
            if ( o is FailoverTransport )
            {
                var oo = o as FailoverTransport;

                return id - oo.id;
            }
            throw new ArgumentException();
        }

        public void Add( Uri[] u )
        {
            lock ( uris )
                for ( var i = 0; i < u.Length; i++ )
                    if ( !uris.Contains( u[i] ) )
                        uris.Add( u[i] );

            Reconnect();
        }

        public void Remove( Uri[] u )
        {
            lock ( uris )
                for ( var i = 0; i < u.Length; i++ )
                    uris.Remove( u[i] );

            Reconnect();
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public Boolean IsStarted { get; private set; }

        public void Start()
        {
            lock ( reconnectMutex )
            {
                if ( IsStarted )
                {
                    Tracer.Debug( "FailoverTransport Already Started." );
                    return;
                }

                Tracer.Debug( "FailoverTransport Started." );
                IsStarted = true;
                if ( ConnectedTransport != null )
                    stateTracker.DoRestore( ConnectedTransport );
                else
                    Reconnect();
            }
        }

        public virtual void Stop()
        {
            ITransport transportToStop = null;

            lock ( reconnectMutex )
            {
                if ( !IsStarted )
                {
                    Tracer.Debug( "FailoverTransport Already Stopped." );
                    return;
                }

                Tracer.Debug( "FailoverTransport Stopped." );
                IsStarted = false;
                IsDisposed = true;
                IsConnected = false;

                if ( ConnectedTransport != null )
                    transportToStop = connectedTransport.GetAndSet( null );
            }

            try
            {
                sleepMutex.WaitOne();
            }
            finally
            {
                sleepMutex.ReleaseMutex();
            }

            reconnectTask?.Shutdown();
            transportToStop?.Stop();
        }

        public FutureResponse AsyncRequest( ICommand command )
        {
            throw new ApplicationException( "FailoverTransport does not implement AsyncRequest(Command)" );
        }

        /// <summary>
        ///     If doing an asynchronous connect, the milliseconds before timing out if no connection can be made
        /// </summary>
        /// <value>The async timeout.</value>
        public Int32 AsyncTimeout { get; set; } = 45000;

        public Boolean IsConnected { get; private set; }

        public Boolean IsFaultTolerant
        {
            get { return true; }
        }

        public Object Narrow( Type type ) => GetType()
            .Equals( type )
            ? this
            : ConnectedTransport?.Narrow( type );

        public void Oneway( ICommand command )
        {
            Exception error = null;

            lock ( reconnectMutex )
            {
                if ( IsShutdownCommand( command ) && ConnectedTransport == null )
                {
                    if ( command.IsShutdownInfo )
                        return;

                    if ( command.IsRemoveInfo )
                    {
                        stateTracker.Track( command );
                        // Simulate response to RemoveInfo command
                        var response = new Response();
                        response.CorrelationId = command.CommandId;
                        OnCommand( this, response );
                        return;
                    }
                }

                // Keep trying until the message is sent.
                for ( var i = 0; !IsDisposed; i++ )
                    try
                    {
                        // Wait for transport to be connected.
                        var transport = ConnectedTransport;
                        var start = DateTime.Now;
                        var timedout = false;
                        while ( transport == null && !IsDisposed && connectionFailure == null )
                        {
                            var elapsed = (Int32) ( DateTime.Now - start ).TotalMilliseconds;
                            if ( Timeout > 0 && elapsed > Timeout )
                            {
                                timedout = true;
                                Tracer.DebugFormat( "FailoverTransport.oneway - timed out after {0} mills", elapsed );
                                break;
                            }

                            // Release so that the reconnect task can run
                            try
                            {
                                // This is a bit of a hack, what should be happening is that when
                                // there's a reconnect the reconnectTask should signal the Monitor
                                // or some other event object to indicate that we can wakeup right
                                // away here, instead of performing the full wait.
                                Monitor.Exit( reconnectMutex );
                                Thread.Sleep( 100 );
                                Monitor.Enter( reconnectMutex );
                            }
                            catch ( Exception e )
                            {
                                Tracer.DebugFormat( "Interrupted: {0}", e.Message );
                            }

                            transport = ConnectedTransport;
                        }

                        if ( transport == null )
                        {
                            // Previous loop may have exited due to use being disposed.
                            if ( IsDisposed )
                                error = new IOException( "Transport disposed." );
                            else if ( connectionFailure != null )
                                error = connectionFailure;
                            else if ( timedout )
                                error = new IOException( "Failover oneway timed out after " + Timeout + " milliseconds." );
                            else
                                error = new IOException( "Unexpected failure." );
                            break;
                        }

                        // If it was a request and it was not being tracked by
                        // the state tracker, then hold it in the requestMap so
                        // that we can replay it later.
                        var tracked = stateTracker.Track( command );
                        lock ( ( (ICollection) requestMap ).SyncRoot )
                            if ( tracked != null && tracked.WaitingForResponse )
                                requestMap.Add( command.CommandId, tracked );
                            else if ( tracked == null && command.ResponseRequired )
                                requestMap.Add( command.CommandId, command );

                        // Send the message.
                        try
                        {
                            transport.Oneway( command );
                            stateTracker.trackBack( command );
                        }
                        catch ( Exception e )
                        {
                            // If the command was not tracked.. we will retry in
                            // this method
                            if ( tracked == null )
                            {
                                // since we will retry in this method.. take it
                                // out of the request map so that it is not
                                // sent 2 times on recovery
                                if ( command.ResponseRequired )
                                    lock ( ( (ICollection) requestMap ).SyncRoot )
                                        requestMap.Remove( command.CommandId );

                                // Rethrow the exception so it will handled by
                                // the outer catch
                                throw e;
                            }
                        }

                        return;
                    }
                    catch ( Exception e )
                    {
                        Tracer.DebugFormat( "Send Oneway attempt: {0} failed: Message = {1}", i, e.Message );
                        Tracer.DebugFormat( "Failed Message Was: {0}", command );
                        HandleTransportFailure( e );
                    }
            }

            if ( !IsDisposed )
                if ( error != null )
                    throw error;
        }

        public Uri RemoteAddress
        {
            get
            {
                if ( ConnectedTransport != null )
                    return ConnectedTransport.RemoteAddress;
                return null;
            }
        }

        public Response Request( ICommand command, TimeSpan ts )
        {
            throw new ApplicationException( "FailoverTransport does not implement Request(Command, TimeSpan)" );
        }

        public void Add( String u )
        {
            try
            {
                var uri = new Uri( u );
                lock ( uris )
                    if ( !uris.Contains( uri ) )
                        uris.Add( uri );

                Reconnect();
            }
            catch ( Exception e )
            {
                Tracer.ErrorFormat( "Failed to parse URI '{0}': {1}", u, e.Message );
            }
        }

        public void Dispose( Boolean disposing )
        {
            if ( disposing )
            {
                // get rid of unmanaged stuff
            }

            IsDisposed = true;
        }

        public void disposedOnCommand( ITransport sender, ICommand c )
        {
        }

        public void disposedOnException( ITransport sender, Exception e )
        {
        }

        public void HandleTransportFailure( Exception e )
        {
            var transport = connectedTransport.GetAndSet( null );
            if ( transport != null )
            {
                transport.Command = disposedOnCommand;
                transport.Exception = disposedOnException;
                try
                {
                    transport.Stop();
                }
                catch ( Exception ex )
                {
                    ex.GetType(); // Ignore errors but this lets us see the error during debugging
                }

                lock ( reconnectMutex )
                {
                    var reconnectOk = false;
                    if ( IsStarted )
                    {
                        Tracer.WarnFormat( "Transport failed to {0}, attempting to automatically reconnect due to: {1}", ConnectedTransportURI.ToString(), e.Message );
                        reconnectOk = true;
                    }

                    failedConnectTransportURI = ConnectedTransportURI;
                    ConnectedTransportURI = null;
                    IsConnected = false;
                    if ( reconnectOk )
                        reconnectTask.Wakeup();
                }

                Interrupted?.Invoke( transport );
            }
        }

        public void OnCommand( ITransport sender, ICommand command )
        {
            if ( command != null )
                if ( command.IsResponse )
                {
                    Object oo = null;
                    lock ( ( (ICollection) requestMap ).SyncRoot )
                    {
                        var v = ( (Response) command ).CorrelationId;
                        try
                        {
                            if ( requestMap.ContainsKey( v ) )
                            {
                                oo = requestMap[v];
                                requestMap.Remove( v );
                            }
                        }
                        catch
                        {
                        }
                    }

                    var t = oo as Tracked;
                    t?.OnResponses();
                }

            Command( sender, command );
        }

        public void OnException( ITransport sender, Exception error )
        {
            try
            {
                HandleTransportFailure( error );
            }
            catch ( Exception e )
            {
                e.GetType();
                // What to do here?
            }
        }

        public void Reconnect( Uri uri ) => Add( new[] { uri } );

        public void Reconnect()
        {
            lock ( reconnectMutex )
                if ( IsStarted )
                {
                    if ( reconnectTask == null )
                    {
                        Tracer.Debug( "Creating reconnect task" );
                        reconnectTask = new DedicatedTaskRunner( new FailoverTask( this ) );
                    }

                    Tracer.Debug( "Waking up reconnect task" );
                    try
                    {
                        reconnectTask.Wakeup();
                    }
                    catch ( Exception )
                    {
                    }
                }
                else
                {
                    Tracer.Debug( "Reconnect was triggered but transport is not started yet. Wait for start to connect the transport." );
                }
        }

        public Response Request( ICommand command )
        {
            throw new ApplicationException( "FailoverTransport does not implement Request(Command)" );
        }

        public override String ToString() => ConnectedTransportURI == null ? "unconnected" : ConnectedTransportURI.ToString();

        protected void RestoreTransport( ITransport t )
        {
            Tracer.Info( "Restoring previous transport connection." );
            t.Start();

            stateTracker.DoRestore( t );

            Tracer.Info( "Sending queued commands..." );
            Dictionary<Int32, ICommand> tmpMap = null;
            lock ( ( (ICollection) requestMap ).SyncRoot )
                tmpMap = new Dictionary<Int32, ICommand>( requestMap );

            foreach ( var command in tmpMap.Values )
                t.Oneway( command );
        }

        private Boolean DoConnect()
        {
            lock ( reconnectMutex )
                if ( ConnectedTransport != null || IsDisposed || connectionFailure != null )
                {
                    return false;
                }
                else
                {
                    var connectList = ConnectList;
                    if ( connectList.Count == 0 )
                    {
                        Failure = new NMSConnectionException( "No URIs available for connection." );
                    }
                    else
                    {
                        if ( !UseExponentialBackOff )
                            ReconnectDelay = InitialReconnectDelay;

                        ITransport transport = null;
                        Uri chosenUri = null;

                        try
                        {
                            foreach ( var uri in connectList )
                            {
                                if ( ConnectedTransport != null || IsDisposed )
                                    break;

                                Tracer.DebugFormat( "Attempting connect to: {0}", uri );

                                // synchronous connect
                                try
                                {
                                    Tracer.DebugFormat( "Attempting connect to: {0}", uri.ToString() );
                                    transport = _transportFactory.CompositeConnect( uri );
                                    chosenUri = transport.RemoteAddress;
                                    break;
                                }
                                catch ( Exception e )
                                {
                                    Failure = e;
                                    Tracer.DebugFormat( "Connect fail to: {0}, reason: {1}", uri, e.Message );
                                }
                            }

                            if ( transport != null )
                            {
                                transport.Command = OnCommand;
                                transport.Exception = OnException;
                                transport.Start();

                                if ( IsStarted )
                                    RestoreTransport( transport );

                                Resumed?.Invoke( transport );

                                Tracer.Debug( "Connection established" );
                                ReconnectDelay = InitialReconnectDelay;
                                ConnectedTransportURI = chosenUri;
                                ConnectedTransport = transport;
                                connectFailures = 0;
                                IsConnected = true;

                                if ( firstConnection )
                                {
                                    firstConnection = false;
                                    Tracer.InfoFormat( "Successfully connected to: {0}", chosenUri.ToString() );
                                }
                                else
                                {
                                    Tracer.InfoFormat( "Successfully reconnected to: {0}", chosenUri.ToString() );
                                }

                                return false;
                            }
                        }
                        catch ( Exception e )
                        {
                            Failure = e;
                            Tracer.DebugFormat( "Connect attempt failed.  Reason: {0}", e.Message );
                        }
                    }

                    var maxAttempts = 0;
                    if ( firstConnection )
                        if ( StartupMaxReconnectAttempts != 0 )
                            maxAttempts = StartupMaxReconnectAttempts;
                    if ( maxAttempts == 0 )
                        maxAttempts = MaxReconnectAttempts;

                    if ( maxAttempts > 0 && ++connectFailures >= maxAttempts )
                    {
                        Tracer.ErrorFormat( "Failed to connect to transport after {0} attempt(s)", connectFailures );
                        connectionFailure = Failure;
                        Exception( this, connectionFailure );
                        return false;
                    }
                }

            if ( !IsDisposed )
            {
                Tracer.DebugFormat( "Waiting {0}ms before attempting connection.", ReconnectDelay );
                lock ( sleepMutex )
                    try
                    {
                        Thread.Sleep( ReconnectDelay );
                    }
                    catch ( Exception )
                    {
                    }

                if ( UseExponentialBackOff )
                {
                    // Exponential increment of reconnect delay.
                    ReconnectDelay *= ReconnectDelayExponent;
                    if ( ReconnectDelay > MaxReconnectDelay )
                        ReconnectDelay = MaxReconnectDelay;
                }
            }
            return !IsDisposed;
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns true if the command is one sent when a connection is being closed.</returns>
        private static Boolean IsShutdownCommand( ICommand command ) => command != null && ( command.IsShutdownInfo || command is RemoveInfo );

        ~FailoverTransport()
        {
            Dispose( false );
        }

        #region Nested Types

        #region FailoverTask

        private class FailoverTask : Task
        {
            #region Fields

            private readonly FailoverTransport parent;

            #endregion

            #region Ctor

            public FailoverTask( FailoverTransport p )
            {
                parent = p;
            }

            #endregion

            public Boolean Iterate()
            {
                var result = false;
                var doReconnect = !parent.IsDisposed && parent.connectionFailure == null;
                try
                {
                    if ( parent.ConnectedTransport == null && doReconnect )
                        result = parent.DoConnect();
                }
                finally
                {
                }

                return result;
            }
        }

        #endregion

        #endregion

        #region Property Accessors

        /// <summary>
        ///     Delegate invoked when a command was received.
        /// </summary>
        public Action<ITransport, ICommand> Command { get; set; }

        /// <summary>
        ///     Delegate invoked when a exception occurs.
        /// </summary>
        public Action<ITransport, Exception> Exception { get; set; }

        /// <summary>
        ///     Delegate invoked when the connection is interrupted.
        /// </summary>
        public Action<ITransport> Interrupted { get; set; }

        /// <summary>
        ///     Delegate invoked when the connection is resumed.
        /// </summary>
        public Action<ITransport> Resumed { get; set; }

        internal Exception Failure
        {
            get { return failure; }
            set
            {
                lock ( mutex )
                    failure = value;
            }
        }

        public Int32 Timeout { get; set; } = -1;

        public Int32 InitialReconnectDelay { get; set; } = 10;

        public Int32 MaxReconnectDelay { get; set; } = 1000 * 30;

        public Int32 ReconnectDelay { get; set; } = 10;

        public Int32 ReconnectDelayExponent { get; set; } = 2;

        public ITransport ConnectedTransport
        {
            get { return connectedTransport.Value; }
            set { connectedTransport.Value = value; }
        }

        public Uri ConnectedTransportURI { get; set; }

        public Int32 MaxReconnectAttempts { get; set; }

        public Int32 StartupMaxReconnectAttempts { get; set; }

        public Boolean Randomize { get; set; } = true;

        public Boolean UseExponentialBackOff { get; set; } = true;

        #endregion
    }
}