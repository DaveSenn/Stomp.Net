#region Usings

using System;
using System.Threading;
using Extend;

#endregion

namespace Stomp.Net.Stomp.Threads
{
    /// <summary>
    ///     A TaskRunner that dedicates a single thread to running a single Task.
    /// </summary>
    public class DedicatedTaskRunner : ITaskRunner
    {
        #region Fields

        private readonly ManualResetEvent _isShutdown = new ManualResetEvent( true );
        private readonly Mutex _mutex = new Mutex();
        private readonly ITask _task;
        private readonly Thread _theThread;
        private readonly AutoResetEvent _waiter = new AutoResetEvent( false );
        private Boolean _pending;
        private Boolean _shutdown;

        private Boolean _terminated;

        #endregion

        #region Ctor

        public DedicatedTaskRunner( ITask task )
        {
            task.ThrowIfNull( nameof(task) );

            _task = task;
            _theThread = new Thread( Run ) { IsBackground = true };
            _theThread.Start();
        }

        #endregion

        public void Shutdown()
        {
            Monitor.Enter( _mutex );

            _shutdown = true;
            _pending = true;

            _waiter.Set();

            // Wait till the thread stops ( no need to wait if shutdown
            // is called from thread that is shutting down)
            if ( Thread.CurrentThread != _theThread && !_terminated )
            {
                Monitor.Exit( _mutex );
                _isShutdown.WaitOne();
            }
            else
                Monitor.Exit( _mutex );
        }

        public void Wakeup()
        {
            lock ( _mutex )
            {
                if ( _shutdown )
                    return;

                _pending = true;

                _waiter.Set();
            }
        }

        private void Run()
        {
            lock ( _mutex )
                _isShutdown.Reset();

            try
            {
                while ( true )
                {
                    lock ( _mutex )
                    {
                        _pending = false;

                        if ( _shutdown )
                            return;
                    }

                    if ( _task.Iterate() )
                        continue;
                    // wait to be notified.
                    Monitor.Enter( _mutex );
                    if ( _shutdown )
                        return;

                    while ( !_pending )
                    {
                        Monitor.Exit( _mutex );
                        _waiter.WaitOne();
                        Monitor.Enter( _mutex );
                    }

                    Monitor.Exit( _mutex );
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                // Make sure we notify any waiting threads that thread
                // has terminated.
                Monitor.Enter( _mutex );
                _terminated = true;
                Monitor.Exit( _mutex );
                _isShutdown.Set();
            }
        }
    }
}