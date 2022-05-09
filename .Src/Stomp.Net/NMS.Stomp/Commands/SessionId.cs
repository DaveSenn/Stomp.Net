#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands;

public class SessionId : BaseDataStructure
{
    public override Boolean Equals( Object that )
    {
        if ( that is SessionId id )
            return Equals( id );

        return false;
    }

    /// <summery>
    ///     Get the unique identifier that this object and its own
    ///     Marshaler share.
    /// </summery>
    public override Byte GetDataStructureType() => DataStructureTypes.SessionIdType;

    public override Int32 GetHashCode()
    {
        var answer = 0;

        answer = answer * 37 + HashCode( ConnectionId );
        answer = answer * 37 + HashCode( Value );

        return answer;
    }

    /// <summery>
    ///     Returns a string containing the information for this DataStructure
    ///     such as its type and value of its elements.
    /// </summery>
    public override String ToString()
        => ConnectionId + ":" + Value;

    protected virtual Boolean Equals( SessionId that )
        => Equals( ConnectionId, that.ConnectionId ) && Equals( Value, that.Value );

    #region Properties

    public String ConnectionId { get; }

    public Int64 Value { get; }

    #endregion

    #region Ctor

    public SessionId( Int64 sessionId, ConnectionId connectionId )
    {
        ConnectionId = connectionId.Value;
        Value = sessionId;
    }

    public SessionId( ConsumerId consumerId )
    {
        ConnectionId = consumerId.ConnectionId;
        Value = consumerId.SessionId;
    }

    #endregion
}