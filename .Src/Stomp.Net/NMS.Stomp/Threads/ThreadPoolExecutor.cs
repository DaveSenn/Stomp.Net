#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using Extend;

#endregion

namespace Stomp.Net.Stomp.Threads;

/// <summary>
///     This class provides a wrapper around the ThreadPool mechanism in .NET
///     to allow for serial execution of jobs in the ThreadPool and provide
///     a means of shutting down the execution of jobs in a deterministic
///     way.
/// </summary>
public class ThreadPoolExecutor
{
    public void QueueUserWorkItem( WaitCallback worker, Object arg )
    {
        worker.ThrowIfNull( nameof(worker) );

        lock ( _syncRoot )
        {
            _workQueue.Enqueue( new(worker, arg) );

            if ( _running )
                return;
            _executionComplete.Reset();
            _running = true;
            ThreadPool.QueueUserWorkItem( QueueProcessor, null );
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
    }

    #region Nested Types

    /// <summary>
    ///     Represents an asynchronous task that is executed on the ThreadPool
    ///     at some point in the future.
    /// </summary>
    private class Future
    {
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
                throw new("Future executed with null WaitCallback");

            try
            {
                _callback( _callbackArg );
            }
            catch
            {
                // ignored
            }
        }

        #region Fields

        private readonly WaitCallback _callback;
        private readonly Object _callbackArg;

        #endregion
    }

    #endregion

    #region Fields

    private readonly ManualResetEvent _executionComplete = new(true);
    private readonly Mutex _syncRoot = new();
    private readonly Queue<Future> _workQueue = new();
    private Boolean _running;

    #endregion
}