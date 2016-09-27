#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
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
        
        Boolean IsMessage { get; }

        Boolean IsMessageAck { get; }

        Boolean IsMessageDispatch { get; }

        Boolean IsRemoveInfo { get; }

        Boolean IsRemoveSubscriptionInfo { get; }

        Boolean IsResponse { get; }

        Boolean IsKeepAliveInfo { get; }

        Boolean IsShutdownInfo { get; }

        Boolean IsWireFormatInfo { get; }

        #endregion

        Response Visit( ICommandVisitor visitor );
    }
}