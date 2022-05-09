#region Usings

#endregion

#region Usings

using System;

#endregion

namespace Stomp.Net.Util;

/// <summary>
///     Base Utility class for conversion between IMessage type objects for different
///     NMS providers.
/// </summary>
public abstract class MessageTransformation
{
    public T TransformMessage<T>( IBytesMessage message )
    {
        if ( message is T variable )
            return variable;

        var msg = DoCreateBytesMessage();

        try
        {
            // ReSharper disable once PossibleNullReferenceException
            msg.Content = new Byte[message.Content.Length];
            Array.Copy( message.Content, msg.Content, message.Content.Length );
        }
        catch
        {
            // ignored
        }

        CopyProperties( message, msg );

        // Let the subclass have a chance to do any last minute configurations
        // on the newly converted message.
        DoPostProcessMessage( msg );

        return (T) msg;
    }

    /// <summary>
    ///     Copies the standard NMS and user defined properties from the given
    ///     message to the specified message, the class version transforms the
    ///     Destination instead of just doing a straight copy.
    /// </summary>
    private void CopyProperties( IBytesMessage fromMessage, IBytesMessage toMessage )
    {
        toMessage.StompMessageId = fromMessage.StompMessageId;
        toMessage.StompCorrelationId = fromMessage.StompCorrelationId;
        toMessage.StompReplyTo = DoTransformDestination( fromMessage.StompReplyTo );
        toMessage.StompDestination = DoTransformDestination( fromMessage.StompDestination );
        toMessage.StompDeliveryMode = fromMessage.StompDeliveryMode;
        toMessage.StompRedelivered = fromMessage.StompRedelivered;
        toMessage.StompType = fromMessage.StompType;
        toMessage.StompPriority = fromMessage.StompPriority;
        toMessage.StompTimestamp = fromMessage.StompTimestamp;
        toMessage.StompTimeToLive = fromMessage.StompTimeToLive;

        foreach ( var x in fromMessage.Headers )
            toMessage.Headers[x.Key] = fromMessage.Headers[x.Value];
    }

    #region Creation Methods and Conversion Support Methods

    protected abstract IBytesMessage DoCreateBytesMessage();

    protected abstract IDestination DoTransformDestination( IDestination destination );
    protected abstract void DoPostProcessMessage( IBytesMessage message );

    #endregion
}