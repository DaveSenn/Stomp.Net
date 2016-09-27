#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Threads
{
    /// <summary>
    ///     Allows you to request a thread execute the associated Task.
    /// </summary>
    public interface TaskRunner
    {
        void Shutdown();
        void Shutdown( TimeSpan timeout );
        void Wakeup();
    }
}