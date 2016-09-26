#region Usings

using System;
using System.Threading;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     A Transport which guards access to the next transport using a mutex.
    /// </summary>
    public class MutexTransport : TransportFilter
    {
        #region Fields

        private readonly Object transmissionLock = new Object();

        #endregion

        #region Ctor

        public MutexTransport( ITransport next )
            : base( next )
        {
        }

        #endregion

        public override FutureResponse AsyncRequest( Command command )
        {
            GetTransmissionLock( next.AsyncTimeout );
            try
            {
                return base.AsyncRequest( command );
            }
            finally
            {
                Monitor.Exit( transmissionLock );
            }
        }

        public override void Oneway( Command command )
        {
            GetTransmissionLock( next.Timeout );
            try
            {
                base.Oneway( command );
            }
            finally
            {
                Monitor.Exit( transmissionLock );
            }
        }

        public override Response Request( Command command, TimeSpan timeout )
        {
            GetTransmissionLock( (Int32) timeout.TotalMilliseconds );
            try
            {
                return base.Request( command, timeout );
            }
            finally
            {
                Monitor.Exit( transmissionLock );
            }
        }

        private void GetTransmissionLock( Int32 timeout )
        {
            if ( timeout > 0 )
            {
                var timeoutTime = DateTime.Now + TimeSpan.FromMilliseconds( timeout );
                var waitCount = 1;

                while ( true )
                {
                    if ( Monitor.TryEnter( transmissionLock ) )
                        break;

                    if ( DateTime.Now > timeoutTime )
                        throw new IOException( String.Format( "Oneway timed out after {0} milliseconds.", timeout ) );

                    // Back off from being overly aggressive.  Having too many threads
                    // aggressively trying to get the lock pegs the CPU.
                    Thread.Sleep( 3 * waitCount++ );
                }
            }
            else
            {
                Monitor.Enter( transmissionLock );
            }
        }
    }
}