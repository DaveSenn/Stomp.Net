#region Usings

using System;
using System.Threading;

#endregion

namespace Stomp.Net.Util
{
    public class CountDownLatch
    {
        #region Fields

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent( false );
        private Int32 _remaining;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current count for this Latch.
        /// </summary>
        public Int32 Remaining
        {
            get
            {
                lock ( _resetEvent )
                    return _remaining;
            }
        }

        public WaitHandle AsyncWaitHandle => _resetEvent;

        #endregion

        #region Ctor

        public CountDownLatch( Int32 i ) => _remaining = i;

        #endregion

        /// <summary>
        ///     Causes the current thread to wait until the latch has counted down to zero, unless
        ///     the thread is interrupted, or the specified waiting time elapses.
        /// </summary>
        public Boolean AwaitOperation(TimeSpan timeout) 
            => _resetEvent.WaitOne( (Int32) timeout.TotalMilliseconds );

        /// <summary>
        ///     Decrement the count, releasing any waiting Threads when the count reaches Zero.
        /// </summary>
        public void CountDown()
        {
            lock ( _resetEvent )
                if ( _remaining > 0 )
                {
                    _remaining--;
                    if ( 0 == _remaining )
                        _resetEvent.Set();
                }
        }
    }
}