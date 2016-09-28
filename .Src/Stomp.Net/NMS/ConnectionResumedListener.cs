namespace Stomp.Net
{
    /// <summary>
    ///     A delegate that is used by Fault tolerant NMS Implementation to notify their
    ///     clients that the Connection that was interrupted has now been restored.
    /// </summary>
    public delegate void ConnectionResumedListener();
}