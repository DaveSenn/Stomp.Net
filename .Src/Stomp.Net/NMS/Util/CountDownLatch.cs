

#region Usings

using System;
using System.Threading;

#endregion

namespace Apache.NMS.Util
{
    public class CountDownLatch
    {
        #region Fields

        private readonly ManualResetEvent mutex = new ManualResetEvent( false );
        private Int32 remaining;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current count for this Latch.
        /// </summary>
        public Int32 Remaining
        {
            get
            {
                lock ( mutex )
                    return remaining;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return mutex; }
        }

        #endregion

        #region Ctor

        public CountDownLatch( Int32 i )
        {
            remaining = i;
        }

        #endregion

        /// <summary>
        ///     Causes the current Thread to wait for the count to reach zero, unless
        ///     the Thread is interrupted.
        /// </summary>
        public void await()
        {
            await( TimeSpan.FromMilliseconds( Timeout.Infinite ) );
        }

        /// <summary>
        ///     Causes the current thread to wait until the latch has counted down to zero, unless
        ///     the thread is interrupted, or the specified waiting time elapses.
        /// </summary>
        public Boolean await( TimeSpan timeout ) => mutex.WaitOne( (Int32) timeout.TotalMilliseconds, false );

        /// <summary>
        ///     Decrement the count, releasing any waiting Threads when the count reaches Zero.
        /// </summary>
        public void countDown()
        {
            lock ( mutex )
                if ( remaining > 0 )
                {
                    remaining--;
                    if ( 0 == remaining )
                        mutex.Set();
                }
        }
    }
}