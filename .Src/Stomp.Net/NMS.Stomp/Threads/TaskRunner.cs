#region Usings



#endregion

namespace Apache.NMS.Stomp.Threads
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