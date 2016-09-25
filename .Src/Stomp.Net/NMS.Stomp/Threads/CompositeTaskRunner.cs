

#region Usings

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Apache.NMS.Stomp.Threads
{
    /// <summary>
    ///     A TaskRunner that dedicates a single thread to running a single Task.
    /// </summary>
    public class CompositeTaskRunner : TaskRunner
    {
        #region Fields

        private readonly ManualResetEvent isShutdown = new ManualResetEvent( true );
        private readonly Mutex mutex = new Mutex();
        private readonly LinkedList<CompositeTask> tasks = new LinkedList<CompositeTask>();

        private readonly Thread theThread;
        private readonly AutoResetEvent waiter = new AutoResetEvent( false );
        private Boolean pending;
        private Boolean shutdown;

        private Boolean terminated;

        #endregion

        #region Ctor

        public CompositeTaskRunner()
        {
            theThread = new Thread( Run );
            theThread.IsBackground = true;
            theThread.Start();
        }

        #endregion

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

        public void AddTask( CompositeTask task )
        {
            lock ( mutex )
            {
                tasks.AddLast( task );
                Wakeup();
            }
        }

        public void RemoveTask( CompositeTask task )
        {
            lock ( mutex )
            {
                tasks.Remove( task );
                Wakeup();
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

                    if ( !Iterate() )
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

        private Boolean Iterate()
        {
            lock ( mutex )
                foreach ( var task in tasks )
                    if ( task.IsPending )
                    {
                        task.Iterate();

                        // Always return true here so that we can check the next
                        // task in the list to see if its done.
                        return true;
                    }

            return false;
        }
    }
}