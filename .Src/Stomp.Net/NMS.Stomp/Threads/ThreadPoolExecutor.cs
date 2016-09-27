#region Usings

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Apache.NMS.Stomp.Threads
{
    /// <summary>
    ///     This class provides a wrapper around the ThreadPool mechanism in .NET
    ///     to allow for serial execution of jobs in the ThreadPool and provide
    ///     a means of shutting down the execution of jobs in a deterministic
    ///     way.
    /// </summary>
    public class ThreadPoolExecutor
    {
        #region Fields

        private readonly ManualResetEvent _executionComplete = new ManualResetEvent( true );
        private readonly Mutex _syncRoot = new Mutex();
        private readonly Queue<Future> _workQueue = new Queue<Future>();
        private Boolean _running;

        #endregion

        #region Properties

        public Boolean IsShutdown { get; private set; }

        #endregion

        public void QueueUserWorkItem( WaitCallback worker, Object arg )
        {
            if ( worker == null )
                throw new ArgumentNullException( "Invalid WaitCallback passed" );

            if ( !IsShutdown )
                lock ( _syncRoot )
                    if ( !IsShutdown )
                    {
                        _workQueue.Enqueue( new Future( worker, arg ) );

                        if ( !_running )
                        {
                            _executionComplete.Reset();
                            _running = true;
                            ThreadPool.QueueUserWorkItem( QueueProcessor, null );
                        }
                    }
        }

        private void QueueProcessor( Object unused )
        {
            Future theTask;

            lock ( _syncRoot )
            {
                if ( _workQueue.Count == 0 )
                {
                    _running = false;
                    _executionComplete.Set();
                    return;
                }

                theTask = _workQueue.Dequeue();
            }

            try
            {
                theTask.Run();
            }
            finally
            {
                ThreadPool.QueueUserWorkItem( QueueProcessor, null );
            }
            /*
            finally
            {
                if ( _closing )
                {
                    _running = false;
                    _executionComplete.Set();
                }
                else
                {
                    ThreadPool.QueueUserWorkItem( QueueProcessor, null );
                }
            }*/
        }

        #region Nested Types

        /// <summary>
        ///     Represents an asynchronous task that is executed on the ThreadPool
        ///     at some point in the future.
        /// </summary>
        private class Future
        {
            #region Fields

            private readonly WaitCallback _callback;
            private readonly Object _callbackArg;

            #endregion

            #region Ctor

            public Future( WaitCallback callback, Object arg )
            {
                _callback = callback;
                _callbackArg = arg;
            }

            #endregion

            public void Run()
            {
                if ( _callback == null )
                    throw new Exception( "Future executed with null WaitCallback" );

                try
                {
                    _callback( _callbackArg );
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion
    }
}