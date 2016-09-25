

#region Usings

using System;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     Handles asynchronous responses
    /// </summary>
    public class FutureResponse
    {
        #region Fields

        private readonly CountDownLatch latch = new CountDownLatch( 1 );
        private Response response;

        #endregion

        #region Properties

        public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromMilliseconds( Timeout.Infinite );

        public WaitHandle AsyncWaitHandle
        {
            get { return latch.AsyncWaitHandle; }
        }

        public Response Response
        {
            // Blocks the caller until a value has been set
            get
            {
                lock ( latch )
                    if ( null != response )
                        return response;

                try
                {
                    if ( !latch.await( ResponseTimeout ) && response == null )
                        throw new RequestTimedOutException();
                }
                catch ( RequestTimedOutException e )
                {
                    Tracer.Error( "Caught Timeout Exception while waiting on monitor: " + e );
                    throw;
                }
                catch ( Exception e )
                {
                    Tracer.Error( "Caught Exception while waiting on monitor: " + e );
                }

                lock ( latch )
                    return response;
            }

            set
            {
                lock ( latch )
                    response = value;

                latch.countDown();
            }
        }

        #endregion
    }
}