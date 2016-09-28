namespace Stomp.Net
{
    /// <summary>
    ///     A delegate that is used by Fault tolerant NMS Implementation to notify their
    ///     clients that the Connection is not currently active to due some error.
    /// </summary>
    public delegate void ConnectionInterruptedListener();
}