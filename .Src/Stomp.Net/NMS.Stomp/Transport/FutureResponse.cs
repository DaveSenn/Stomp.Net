#region Usings

using System;
using System.Threading;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Util;

#endregion

namespace Stomp.Net.Stomp.Transport
{
    /// <summary>
    ///     Handles asynchronous responses.
    /// </summary>
    public class FutureResponse
    {
        #region Fields

        private readonly CountDownLatch _latch = new(1);
        private Response _response;

        #endregion

        #region Properties

        public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromMilliseconds( Timeout.Infinite );

        public Response Response
        {
            // Blocks the caller until a value has been set
            get
            {
                lock ( _latch )
                    if ( null != _response )
                        return _response;

                try
                {
                    if ( !_latch.AwaitOperation( ResponseTimeout ) && _response == null )
                        throw new RequestTimedOutException( "Waiting for response timed out." );
                }
                catch ( RequestTimedOutException e )
                {
                    if ( Tracer.IsErrorEnabled )
                        Tracer.Error( "Caught Timeout Exception while waiting on monitor: " + e );
                    throw;
                }
                catch ( Exception e )
                {
                    if ( Tracer.IsErrorEnabled )
                        Tracer.Error( "Caught Exception while waiting on monitor: " + e );
                }

                lock ( _latch )
                    return _response;
            }

            set
            {
                lock ( _latch )
                    _response = value;

                _latch.CountDown();
            }
        }

        #endregion
    }
}