#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     An Stomp command
    /// </summary>
    public interface ICommand : ICloneable
    {
        #region Properties

        [PublicAPI]
        Int32 CommandId { get; set; }

        [PublicAPI]
        Boolean ResponseRequired { get; set; }

        [PublicAPI]
        Boolean IsConnectionInfo { get; }

        [PublicAPI]
        Boolean IsErrorCommand { get; }

        [PublicAPI]
        Boolean IsMessageDispatch { get; }

        [PublicAPI]
        Boolean IsRemoveInfo { get; }

        [PublicAPI]
        Boolean IsResponse { get; }

        [PublicAPI]
        Boolean IsKeepAliveInfo { get; }

        [PublicAPI]
        Boolean IsShutdownInfo { get; }

        [PublicAPI]
        Boolean IsWireFormatInfo { get; }

        #endregion
    }
}