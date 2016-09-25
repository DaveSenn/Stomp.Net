

#region Usings

using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Interface that provides for a Class to provide dispatching service for
    ///     an OpenWire MessageDispatch command.
    /// </summary>
    public interface IDispatcher
    {
        void Dispatch( MessageDispatch messageDispatch );
    }
}