#region Usings

using System;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.Protocol;
using Apache.NMS.Stomp.Threads;
using Apache.NMS.Stomp.Util;
using Apache.NMS.Util;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     This class make sure that the connection is still alive,
    ///     by monitoring the reception of commands from the peer of
    ///     the transport.
    /// </summary>
    public class InactivityMonitor : TransportFilter
    {
        #region Constants

        private static Int32 id;

        #endregion

        #region Fields

        private readonly Atomic<Boolean> commandReceived = new Atomic<Boolean>( false );

        private readonly Atomic<Boolean> commandSent = new Atomic<Boolean>( false );

        private readonly Atomic<Boolean> failed = new Atomic<Boolean>( false );
        private readonly Atomic<Boolean> inRead = new Atomic<Boolean>( false );
        private readonly Int32 instanceId;
        private readonly Atomic<Boolean> inWrite = new Atomic<Boolean>( false );

        private readonly Mutex monitor = new Mutex();
        private readonly Atomic<Boolean> monitorStarted = new Atomic<Boolean>( false );
        private AsyncSignalReadErrorkTask asyncErrorTask;

        private CompositeTaskRunner asyncTasks;
        private AsyncWriteTask asyncWriteTask;

        private Timer connectionCheckTimer;
        private Boolean disposing;

        private DateTime lastReadCheckTime;

        // Local and remote Wire Format Information
        private StompWireFormat localWireFormatInfo;

        private WireFormatInfo remoteWireFormatInfo;

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
            instanceId = ++id;
            localWireFormatInfo = wireFormat;
        }

        #endregion

        public override void Oneway( ICommand command )
        {
            // Disable inactivity monitoring while processing a command.
            //synchronize this method - its not synchronized
            //further down the transport stack and gets called by more
            //than one thread  by this class
            lock ( inWrite )
            {
                inWrite.Value = true;
                try
                {
                    if ( failed.Value )
                        throw new IoException( "Channel was inactive for too long: " + next.RemoteAddress );
                    if ( command.IsConnectionInfo )
                        lock ( monitor )
                            StartMonitorThreads();
                    next.Oneway( command );
                }
                finally
                {
                    commandSent.Value = true;
                    inWrite.Value = false;
                }
            }
        }

        public override void Stop()
        {
            StopMonitorThreads();
            next.Stop();
        }

        #region WriteCheck Related

        /// <summary>
        ///     Check the write to the broker
        /// </summary>
        public void WriteCheck()
        {
            if ( inWrite.Value || failed.Value )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: is in write or already failed.", instanceId );
                return;
            }

            if ( !commandSent.Value )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: No Message sent since last write check. Sending a KeepAliveInfo.", instanceId );
                asyncWriteTask.IsPending = true;
                asyncTasks.Wakeup();
            }

            commandSent.Value = false;
        }

        #endregion

        protected override void Dispose( Boolean disposing )
        {
            if ( disposing )
            {
                // get rid of unmanaged stuff
            }

            lock ( monitor )
            {
                localWireFormatInfo = null;
                remoteWireFormatInfo = null;
                this.disposing = true;
                StopMonitorThreads();
            }

            base.Dispose( disposing );
        }

        protected override void OnCommand( ITransport sender, ICommand command )
        {
            commandReceived.Value = true;
            inRead.Value = true;
            try
            {
                if ( command.IsWireFormatInfo )
                    lock ( monitor )
                    {
                        remoteWireFormatInfo = command as WireFormatInfo;
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
                inRead.Value = false;
            }
        }

        protected override void OnException( ITransport sender, Exception command )
        {
            if ( failed.CompareAndSet( false, true ) && !disposing )
            {
                Tracer.WarnFormat( "Exception received in the Inactivity Monitor: {0}", command.Message );
                StopMonitorThreads();
                base.OnException( sender, command );
            }
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
            lock ( monitor )
            {
                if ( IsDisposed || disposing )
                    return;

                if ( monitorStarted.Value )
                    return;

                if ( localWireFormatInfo == null )
                    return;

                if ( remoteWireFormatInfo == null )
                    return;

                if ( localWireFormatInfo.MaxInactivityDuration != 0 &&
                     remoteWireFormatInfo.WriteCheckInterval != 0 )
                {
                    ReadCheckTime =
                        Math.Max(
                            localWireFormatInfo.ReadCheckInterval,
                            remoteWireFormatInfo.WriteCheckInterval );

                    asyncErrorTask = new AsyncSignalReadErrorkTask( this, next.RemoteAddress );
                }

                if ( localWireFormatInfo.MaxInactivityDuration != 0 )
                {
                    if ( remoteWireFormatInfo.Version > 1.0 )
                        WriteCheckTime =
                            Math.Max( localWireFormatInfo.WriteCheckInterval,
                                      remoteWireFormatInfo.ReadCheckInterval );
                    else
                        WriteCheckTime = localWireFormatInfo.WriteCheckInterval;

                    asyncWriteTask = new AsyncWriteTask( this );
                }

                InitialDelayTime = localWireFormatInfo.MaxInactivityDurationInitialDelay > 0
                    ? localWireFormatInfo.MaxInactivityDurationInitialDelay
                    : WriteCheckTime;

                asyncTasks = new CompositeTaskRunner();

                if ( asyncErrorTask != null )
                    asyncTasks.AddTask( asyncErrorTask );

                if ( asyncWriteTask != null )
                {
                    Tracer.WarnFormat( "InactivityMonitor[{0}]: Write Check time interval: {1}",
                                       instanceId,
                                       WriteCheckTime );
                    asyncTasks.AddTask( asyncWriteTask );
                }

                if ( asyncErrorTask == null && asyncWriteTask == null )
                    return;

                Tracer.WarnFormat( "InactivityMonitor[{0}]: Starting the Monitor Timer.", instanceId );
                monitorStarted.Value = true;

                connectionCheckTimer = new Timer(
                    CheckConnection,
                    null,
                    InitialDelayTime,
                    WriteCheckTime
                );
            }
        }

        private void StopMonitorThreads()
        {
            lock ( monitor )
                if ( monitorStarted.CompareAndSet( true, false ) )
                {
                    // Attempt to wait for the Timer to shutdown, but don't wait
                    // forever, if they don't shutdown after two seconds, just quit.
                    ThreadUtil.DisposeTimer( connectionCheckTimer, 2000 );

                    asyncTasks.Shutdown();
                    asyncTasks = null;
                    asyncWriteTask = null;
                    asyncErrorTask = null;
                }
        }

        ~InactivityMonitor()
        {
            Dispose( false );
        }

        #region ReadCheck Related

        public void ReadCheck()
        {
            var now = DateTime.Now;
            var elapsed = now - lastReadCheckTime;

            if ( !AllowReadCheck( elapsed ) )
                return;

            lastReadCheckTime = now;

            if ( inRead.Value || failed.Value || asyncErrorTask == null )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: A receive is in progress or already failed.", instanceId );
                return;
            }

            if ( !commandReceived.Value )
            {
                Tracer.WarnFormat( "InactivityMonitor[{0}]: No message received since last read check! Sending an InactivityException!", instanceId );
                asyncErrorTask.IsPending = true;
                asyncTasks.Wakeup();
            }
            else
            {
                commandReceived.Value = false;
            }
        }

        /// <summary>
        ///     Checks if we should allow the read check(if less than 90% of the read
        ///     check time elapsed then we dont do the readcheck
        /// </summary>
        /// <param name="elapsed"></param>
        /// <returns></returns>
        public Boolean AllowReadCheck( TimeSpan elapsed ) => elapsed.TotalMilliseconds > ReadCheckTime;

        #endregion

        #region Async Tasks

        // Task that fires when the TaskRunner is signaled by the ReadCheck Timer Task.
        private class AsyncSignalReadErrorkTask : CompositeTask
        {
            #region Fields

            private readonly InactivityMonitor parent;
            private readonly Atomic<Boolean> pending = new Atomic<Boolean>( false );
            private readonly Uri remote;

            #endregion

            #region Ctor

            public AsyncSignalReadErrorkTask( InactivityMonitor parent, Uri remote )
            {
                this.parent = parent;
                this.remote = remote;
            }

            #endregion

            public Boolean IsPending
            {
                get { return pending.Value; }
                set { pending.Value = value; }
            }

            public Boolean Iterate()
            {
                if ( pending.CompareAndSet( true, false ) && parent.monitorStarted.Value )
                {
                    var ex = new IoException( "Channel was inactive for too long: " + remote );
                    parent.OnException( parent, ex );
                }

                return pending.Value;
            }
        }

        // Task that fires when the TaskRunner is signaled by the WriteCheck Timer Task.
        private class AsyncWriteTask : CompositeTask
        {
            #region Fields

            private readonly InactivityMonitor parent;
            private readonly Atomic<Boolean> pending = new Atomic<Boolean>( false );

            #endregion

            #region Ctor

            public AsyncWriteTask( InactivityMonitor parent )
            {
                this.parent = parent;
            }

            #endregion

            public Boolean IsPending
            {
                get { return pending.Value; }
                set { pending.Value = value; }
            }

            public Boolean Iterate()
            {
                if ( !pending.CompareAndSet( true, false ) || !parent.monitorStarted.Value )
                    return pending.Value;

                try
                {
                    var info = new KeepAliveInfo();
                    parent.next.Oneway( info );
                }
                catch ( IoException ex )
                {
                    parent.OnException( parent, ex );
                }

                return pending.Value;
            }
        }

        #endregion
    }
}