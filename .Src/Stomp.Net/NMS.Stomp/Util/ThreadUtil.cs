#region Usings

using System;
using System.Threading;

#endregion

namespace Stomp.Net.Stomp.Util
{
    public static class ThreadUtil
    {
        public static void DisposeTimer( Timer timer, Int32 timeout )
        {
            var shutdownEvent = new AutoResetEvent( false );

            // Attempt to wait for the Timer to shutdown
            timer.Dispose( shutdownEvent );
            shutdownEvent.WaitOne( timeout, false );
        }
    }
}