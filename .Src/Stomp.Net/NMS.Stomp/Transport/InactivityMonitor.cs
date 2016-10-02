#region Usings

using System;
using System.Threading;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Stomp.Protocol;
using Stomp.Net.Stomp.Threads;
using Stomp.Net.Stomp.Util;
using Stomp.Net.Utilities;

#endregion

namespace Stomp.Net.Stomp.Transport
{
    /// <summary>
    ///     This class make sure that the connection is still alive,
    ///     by monitoring the reception of commands from the peer of
    ///     the transport.
    /// </summary>
    public class InactivityMonitor : TransportFilter
    {
        #region Constants

        private static Int32 _id;

        #endregion

        #region Fields

        private readonly Atomic<Boolean> _commandReceived = new Atomic<Boolean>( false );

        private readonly Atomic<Boolean> _commandSent = new Atomic<Boolean>( false );

        private readonly Atomic<Boolean> _failed = new Atomic<Boolean>( false );
        private readonly Atomic<Boolean> _inRead = new Atomic<Boolean>( false );
        private readonly Int32 _instanceId;
        private readonly Atomic<Boolean> _inWrite = new Atomic<Boolean>( false );

        private readonly Mutex _monitor = new Mutex();
        private readonly Atomic<Boolean> _monitorStarted = new Atomic<Boolean>( false );
        private AsyncSignalReadErrorkTask _asyncErrorTask;

        private CompositeTaskRunner _asyncTasks;
        private AsyncWriteTask _asyncWriteTask;

        private Timer _connectionCheckTimer;
        private Boolean _disposing;

        private DateTime _lastReadCheckTime;

        // Local and remote Wire Format Information
        private StompWireFormat _localWireFormatInfo;

        private WireFormatInfo _remoteWireFormatInfo;

        #endregion

        #region Properties

        public Int64 ReadCheckTime { get; set; } = 30000;

        public Int64 WriteCheckTime { get; set; } = 10000;

        public Int64 InitialDelayTime { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        ///     Constructor or the Inactivity Monitor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="wireFormat"></param>
        public InactivityMonitor( ITransport next, StompWireFormat wireFormat )
            : base( next )
        {
            _instanceId = ++_id;
            _localWireFormatInfo = wireFormat;
        }

        #endregion

        


        #region Overrides of Disposable

        /// <summary>
        ///     Method invoked when the instance gets disposed.
        /// </summary>
        protected override void Disposed()
        {
            lock ( _monitor )
            {
                _localWireFormatInfo = null;
                _remoteWireFormatInfo = null;
                _disposing = true;
                StopMonitorThreads();
            }
        }

        #endregion


        public override void Oneway( ICommand command )
        {
            // Disable inactivity monitoring while processing a command.
            //synchronize this method - its not synchronized
            //further down the transport stack and gets called by more
            //than one thread  by this class
            lock ( _inWrite )
            {
                _inWrite.Value = true;
                try
                {
                    if ( _failed.Value )
                        throw new IoException( "Channel was inactive for too long: " + Next.RemoteAddress );
                    if ( command.IsConnectionInfo )
                        lock ( _monitor )
                            StartMonitorThreads();
                    Next.Oneway( command );
                }
                finally
                {
                    _commandSent.Value = true;
                    _inWrite.Value = false;
                }
            }
        }

        public override void Stop()
        {
            StopMonitorThreads();
            Next.Stop();
        }

        #region WriteCheck Related

        /// <summary>
        ///     Check the write to the broker
        /// </summary>
        private void WriteCheck()
        {
            if ( _inWrite.Value || _failed.Value )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: is in write or already failed.", _instanceId );
                return;
            }

            if ( !_commandSent.Value )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: No Message sent since last write check. Sending a KeepAliveInfo.", _instanceId );
                _asyncWriteTask.IsPending = true;
                _asyncTasks.Wakeup();
            }

            _commandSent.Value = false;
        }

        #endregion


        protected override void OnCommand( ITransport sender, ICommand command )
        {
            _commandReceived.Value = true;
            _inRead.Value = true;
            try
            {
                if ( command.IsWireFormatInfo )
                    lock ( _monitor )
                    {
                        _remoteWireFormatInfo = command as WireFormatInfo;
                        try
                        {
                            StartMonitorThreads();
                        }
                        catch ( IoException ex )
                        {
                            OnException( this, ex );
                        }
                    }

                base.OnCommand( sender, command );
            }
            finally
            {
                _inRead.Value = false;
            }
        }

        protected override void OnException( ITransport sender, Exception command )
        {
            if ( !_failed.CompareAndSet( false, true ) || _disposing )
                return;
            Tracer.WarnFormat( "Exception received in the Inactivity Monitor: {0}", command.Message );
            StopMonitorThreads();
            base.OnException( sender, command );
        }

        private void CheckConnection( Object state )
        {
            // First see if we have written or can write.
            WriteCheck();

            // Now check is we've read anything, if not then we send
            // a new KeepAlive with response required.
            ReadCheck();
        }

        private void StartMonitorThreads()
        {
            lock ( _monitor )
            {
                if ( IsDisposed || _disposing )
                    return;

                if ( _monitorStarted.Value )
                    return;

                if ( _localWireFormatInfo == null )
                    return;

                if ( _remoteWireFormatInfo == null )
                    return;

                if ( _localWireFormatInfo.MaxInactivityDuration != 0 &&
                     _remoteWireFormatInfo.WriteCheckInterval != 0 )
                {
                    ReadCheckTime =
                        Math.Max(
                            _localWireFormatInfo.ReadCheckInterval,
                            _remoteWireFormatInfo.WriteCheckInterval );

                    _asyncErrorTask = new AsyncSignalReadErrorkTask( this, Next.RemoteAddress );
                }

                if ( _localWireFormatInfo.MaxInactivityDuration != 0 )
                {
                    if ( _remoteWireFormatInfo.Version > 1.0 )
                        WriteCheckTime =
                            Math.Max( _localWireFormatInfo.WriteCheckInterval,
                                      _remoteWireFormatInfo.ReadCheckInterval );
                    else
                        WriteCheckTime = _localWireFormatInfo.WriteCheckInterval;

                    _asyncWriteTask = new AsyncWriteTask( this );
                }

                InitialDelayTime = _localWireFormatInfo.MaxInactivityDurationInitialDelay > 0
                    ? _localWireFormatInfo.MaxInactivityDurationInitialDelay
                    : WriteCheckTime;

                _asyncTasks = new CompositeTaskRunner();

                if ( _asyncErrorTask != null )
                    _asyncTasks.AddTask( _asyncErrorTask );

                if ( _asyncWriteTask != null )
                {
                    Tracer.WarnFormat( "InactivityMonitor[{0}]: Write Check time interval: {1}",
                                       _instanceId,
                                       WriteCheckTime );
                    _asyncTasks.AddTask( _asyncWriteTask );
                }

                if ( _asyncErrorTask == null && _asyncWriteTask == null )
                    return;

                Tracer.WarnFormat( "InactivityMonitor[{0}]: Starting the Monitor Timer.", _instanceId );
                _monitorStarted.Value = true;

                _connectionCheckTimer = new Timer(
                    CheckConnection,
                    null,
                    InitialDelayTime,
                    WriteCheckTime
                );
            }
        }

        private void StopMonitorThreads()
        {
            lock ( _monitor )
                if ( _monitorStarted.CompareAndSet( true, false ) )
                {
                    // Attempt to wait for the Timer to shutdown, but don't wait
                    // forever, if they don't shutdown after two seconds, just quit.
                    ThreadUtil.DisposeTimer( _connectionCheckTimer, 2000 );

                    _connectionCheckTimer.Dispose();

                    _asyncTasks.Shutdown();
                    _asyncTasks = null;
                    _asyncWriteTask = null;
                    _asyncErrorTask = null;
                }
        }


        #region ReadCheck Related

        private void ReadCheck()
        {
            var now = DateTime.Now;
            var elapsed = now - _lastReadCheckTime;

            if ( !AllowReadCheck( elapsed ) )
                return;

            _lastReadCheckTime = now;

            if ( _inRead.Value || _failed.Value || _asyncErrorTask == null )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: A receive is in progress or already failed.", _instanceId );
                return;
            }

            if ( !_commandReceived.Value )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: No message received since last read check! Sending an InactivityException!", _instanceId );
                _asyncErrorTask.IsPending = true;
                _asyncTasks.Wakeup();
            }
            else
            {
                _commandReceived.Value = false;
            }
        }

        /// <summary>
        ///     Checks if we should allow the read check(if less than 90% of the read
        ///     check time elapsed then we dont do the readcheck
        /// </summary>
        /// <param name="elapsed"></param>
        /// <returns></returns>
        private Boolean AllowReadCheck( TimeSpan elapsed ) => elapsed.TotalMilliseconds > ReadCheckTime;

        #endregion

        #region Async Tasks

        // Task that fires when the TaskRunner is signaled by the ReadCheck Timer Task.
        private class AsyncSignalReadErrorkTask : ICompositeTask
        {
            #region Fields

            private readonly InactivityMonitor _parent;
            private readonly Atomic<Boolean> _pending = new Atomic<Boolean>( false );
            private readonly Uri _remote;

            #endregion

            #region Ctor

            public AsyncSignalReadErrorkTask( InactivityMonitor parent, Uri remote )
            {
                _parent = parent;
                _remote = remote;
            }

            #endregion

            public Boolean IsPending
            {
                get { return _pending.Value; }
                set { _pending.Value = value; }
            }

            public Boolean Iterate()
            {
                if ( !_pending.CompareAndSet( true, false ) || !_parent._monitorStarted.Value )
                    return _pending.Value;

                var ex = new IoException( "Channel was inactive for too long: " + _remote );
                _parent.OnException( _parent, ex );

                return _pending.Value;
            }
        }

        // Task that fires when the TaskRunner is signaled by the WriteCheck Timer Task.
        private class AsyncWriteTask : ICompositeTask
        {
            #region Fields

            private readonly InactivityMonitor _parent;
            private readonly Atomic<Boolean> _pending = new Atomic<Boolean>( false );

            #endregion

            #region Ctor

            public AsyncWriteTask( InactivityMonitor parent )
            {
                _parent = parent;
            }

            #endregion

            public Boolean IsPending
            {
                get { return _pending.Value; }
                set { _pending.Value = value; }
            }

            public Boolean Iterate()
            {
                if ( !_pending.CompareAndSet( true, false ) || !_parent._monitorStarted.Value )
                    return _pending.Value;

                try
                {
                    var info = new KeepAliveInfo();
                    _parent.Next.Oneway( info );
                }
                catch ( IoException ex )
                {
                    _parent.OnException( _parent, ex );
                }

                return _pending.Value;
            }
        }

        #endregion
    }
}