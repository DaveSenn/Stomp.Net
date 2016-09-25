

#region Usings

using System;
using System.Threading;

#endregion

namespace Apache.NMS.Stomp.Threads
{
    /// <summary>
    ///     A TaskRunner that dedicates a single thread to running a single Task.
    /// </summary>
    public class DedicatedTaskRunner : TaskRunner
    {
        #region Fields

        private readonly ManualResetEvent isShutdown = new ManualResetEvent( true );
        private readonly Mutex mutex = new Mutex();
        private readonly Task task;
        private readonly Thread theThread;
        private readonly AutoResetEvent waiter = new AutoResetEvent( false );
        private Boolean pending;
        private Boolean shutdown;

        private Boolean terminated;

        #endregion

        #region Ctor

        public DedicatedTaskRunner( Task task )
        {
            if ( task == null )
                throw new NullReferenceException( "Task was null" );

            this.task = task;

            theThread = new Thread( Run );
            theThread.IsBackground = true;
            theThread.Start();
        }

        #endregion

        public void Shutdown( TimeSpan timeout )
        {
            Monitor.Enter( mutex );

            shutdown = true;
            pending = true;

            waiter.Set();

            // Wait till the thread stops ( no need to wait if shutdown
            // is called from thread that is shutting down)
            if ( Thread.CurrentThread != theThread && !terminated )
            {
                Monitor.Exit( mutex );
                isShutdown.WaitOne( timeout.Milliseconds, false );
            }
            else
            {
                Monitor.Exit( mutex );
            }
        }

        public void Shutdown()
        {
            Monitor.Enter( mutex );

            shutdown = true;
            pending = true;

            waiter.Set();

            // Wait till the thread stops ( no need to wait if shutdown
            // is called from thread that is shutting down)
            if ( Thread.CurrentThread != theThread && !terminated )
            {
                Monitor.Exit( mutex );
                isShutdown.WaitOne();
            }
            else
            {
                Monitor.Exit( mutex );
            }
        }

        public void Wakeup()
        {
            lock ( mutex )
            {
                if ( shutdown )
                    return;

                pending = true;

                waiter.Set();
            }
        }

        internal void Run()
        {
            lock ( mutex )
                isShutdown.Reset();

            try
            {
                while ( true )
                {
                    lock ( mutex )
                    {
                        pending = false;

                        if ( shutdown )
                            return;
                    }

                    if ( !task.Iterate() )
                    {
                        // wait to be notified.
                        Monitor.Enter( mutex );
                        if ( shutdown )
                            return;

                        while ( !pending )
                        {
                            Monitor.Exit( mutex );
                            waiter.WaitOne();
                            Monitor.Enter( mutex );
                        }
                        Monitor.Exit( mutex );
                    }
                }
            }
            catch
            {
            }
            finally
            {
                // Make sure we notify any waiting threads that thread
                // has terminated.
                Monitor.Enter( mutex );
                terminated = true;
                Monitor.Exit( mutex );
                isShutdown.Set();
            }
        }
    }
}