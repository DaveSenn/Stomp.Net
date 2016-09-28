#region Usings

using System;
using System.Threading;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp.Transport
{
    /// <summary>
    ///     A Transport which guards access to the next transport using a mutex.
    /// </summary>
    public class MutexTransport : TransportFilter
    {
        #region Fields

        private readonly Object _transmissionLock = new Object();

        #endregion

        #region Ctor

        public MutexTransport( ITransport next )
            : base( next )
        {
        }

        #endregion

        public override FutureResponse AsyncRequest( ICommand command )
        {
            GetTransmissionLock( Next.AsyncTimeout );
            try
            {
                return base.AsyncRequest( command );
            }
            finally
            {
                Monitor.Exit( _transmissionLock );
            }
        }

        public override void Oneway( ICommand command )
        {
            GetTransmissionLock( Next.Timeout );
            try
            {
                base.Oneway( command );
            }
            finally
            {
                Monitor.Exit( _transmissionLock );
            }
        }

        public override Response Request( ICommand command, TimeSpan timeout )
        {
            GetTransmissionLock( (Int32) timeout.TotalMilliseconds );
            try
            {
                return base.Request( command, timeout );
            }
            finally
            {
                Monitor.Exit( _transmissionLock );
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
                    if ( Monitor.TryEnter( _transmissionLock ) )
                        break;

                    if ( DateTime.Now > timeoutTime )
                        throw new IoException( $"Oneway timed out after {timeout} milliseconds." );

                    // Back off from being overly aggressive.
                    // Having too many threads aggressively trying to get the lock pegs the CPU.
                    Thread.Sleep( 3 * waitCount++ );
                }
            }
            else
            {
                Monitor.Enter( _transmissionLock );
            }
        }
    }
}