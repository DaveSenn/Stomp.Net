#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands;

public class MessageDispatch : BaseCommand
{
    #region Ctor

    public MessageDispatch( ConsumerId consumerId, Destination destination, BytesMessage message, Int32 redeliveryCounter )
    {
        ConsumerId = consumerId;
        Destination = destination;
        Message = message;
        RedeliveryCounter = redeliveryCounter;
    }

    #endregion

    public override Boolean Equals( Object that )
    {
        if ( that is MessageDispatch dispatch )
            return Equals( dispatch );

        return false;
    }

    /// <summery>
    ///     Get the unique identifier that this object and its own
    ///     Marshaler share.
    /// </summery>
    public override Byte GetDataStructureType()
        => DataStructureTypes.MessageDispatchType;

    public override Int32 GetHashCode()
    {
        var answer = 0;

        answer = answer * 37 + HashCode( ConsumerId );
        answer = answer * 37 + HashCode( Destination );
        answer = answer * 37 + HashCode( Message );
        answer = answer * 37 + HashCode( RedeliveryCounter );

        return answer;
    }

    /// <summery>
    ///     Returns a string containing the information for this DataStructure
    ///     such as its type and value of its elements.
    /// </summery>
    public override String ToString()
        => GetType()
               .Name + "[" +
           "ConsumerId=" + ConsumerId + ", " +
           "Destination=" + Destination + ", " +
           "Message=" + Message + ", " +
           "RedeliveryCounter=" + RedeliveryCounter +
           "]";

    protected virtual Boolean Equals( MessageDispatch that )
    {
        if ( !Equals( ConsumerId, that.ConsumerId ) )
            return false;
        if ( !Equals( Destination, that.Destination ) )
            return false;

        return Equals( Message, that.Message ) && Equals( RedeliveryCounter, that.RedeliveryCounter );
    }

    #region Properties

    public ConsumerId ConsumerId { get; }

    public Destination Destination { get; }

    public BytesMessage Message { get; }

    public Int32 RedeliveryCounter { get; }

    /// <summery>
    ///     Return an answer of true to the isMessageDispatch() query.
    /// </summery>
    public override Boolean IsMessageDispatch => true;

    #endregion
}