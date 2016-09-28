#region Usings

#endregion

namespace Stomp.Net.Stomp.Threads
{
    /// <summary>
    ///     Allows you to request a thread execute the associated Task.
    /// </summary>
    public interface ITaskRunner
    {
        void Shutdown();

        void Wakeup();
    }
}