namespace Apache.NMS
{
    /// <summary>
    ///     A lifecycle for NMS objects to indicate they can be stopped
    /// </summary>
    public interface IStoppable
    {
        void Stop();
    }
}