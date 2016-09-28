#region Usings

using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp
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