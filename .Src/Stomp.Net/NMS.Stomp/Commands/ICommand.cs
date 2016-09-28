#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     An Stomp command
    /// </summary>
    public interface ICommand : ICloneable
    {
        #region Properties

        Int32 CommandId { get; set; }

        Boolean ResponseRequired { get; set; }

        Boolean IsConnectionInfo { get; }

        Boolean IsErrorCommand { get; }

        Boolean IsMessageDispatch { get; }

        Boolean IsRemoveInfo { get; }

        Boolean IsResponse { get; }

        Boolean IsKeepAliveInfo { get; }

        Boolean IsShutdownInfo { get; }

        Boolean IsWireFormatInfo { get; }

        #endregion
    }
}