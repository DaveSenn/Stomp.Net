#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Stomp.Commands;

public abstract class BaseCommand : BaseDataStructure, ICommand
{
    public override Object Clone()
    {
        // Since we are a derived class use the base's Clone()
        // to perform the shallow copy. Since it is shallow it
        // will include our derived class. Since we are derived,
        // this method is an override.
        var o = (BaseCommand) base.Clone();

        return o;
    }

    public Int32 CommandId { get; set; }

    public virtual Boolean IsConnectionInfo => false;

    public virtual Boolean IsErrorCommand => false;

    public virtual Boolean IsKeepAliveInfo => false;

    public virtual Boolean IsMessageDispatch => false;

    public virtual Boolean IsRemoveInfo => false;

    public virtual Boolean IsResponse => false;

    public virtual Boolean IsShutdownInfo => false;

    public virtual Boolean IsWireFormatInfo => false;

    public virtual Boolean ResponseRequired { get; set; }

    public override Boolean Equals( Object that )
    {
        if ( that is not BaseCommand thatCommand )
            return false;

        return GetDataStructureType() == thatCommand.GetDataStructureType()
               && CommandId == thatCommand.CommandId;
    }

    public override Int32 GetHashCode()
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        => CommandId * 37 + GetDataStructureType();

    public override String ToString()
    {
        var answer = DataStructureTypes.GetDataStructureTypeAsString( GetDataStructureType() );
        if ( answer.Length == 0 )
            answer = base.ToString();

        return answer + ": id = " + CommandId;
    }

    #region Properties

    [PublicAPI]
    public virtual Boolean IsMessage => false;

    [PublicAPI]
    public virtual Boolean IsMessageAck => false;

    [PublicAPI]
    public virtual Boolean IsRemoveSubscriptionInfo => false;

    #endregion
}