

#region Usings

using System;
using System.Threading;

#endregion

namespace Apache.NMS.Stomp.Util
{
    public class ThreadUtil
    {
        public static void DisposeTimer( Timer timer, Int32 timeout )
        {
#if NETCF
            timer.Dispose();
#else
            var shutdownEvent = new AutoResetEvent( false );

            // Attempt to wait for the Timer to shutdown
            timer.Dispose( shutdownEvent );
            shutdownEvent.WaitOne( timeout, false );
#endif
        }

        public static void WaitAny( WaitHandle[] waitHandles, Int32 millisecondsTimeout, Boolean exitContext )
        {
#if NETCF
// TODO: Implement .NET CF version of WaitAny().
#else
            WaitHandle.WaitAny( waitHandles, millisecondsTimeout, exitContext );
#endif
        }
    }
}