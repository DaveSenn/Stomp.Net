#region Usings

using System;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp.Transport;

/// <summary>
///     A Transport filter that is used to log the commands sent and received.
/// </summary>
public class LoggingTransport : TransportFilter
{
    #region Ctor

    public LoggingTransport( ITransport next )
        : base( next )
    {
    }

    #endregion

    public override void Oneway( ICommand command )
    {
        if ( Tracer.IsInfoEnabled )
            Tracer.Info( "SENDING: " + command );

        Next.Oneway( command );
    }

    #region Overrides of Disposable

    /// <summary>
    ///     Method invoked when the instance gets disposed.
    /// </summary>
    protected override void Disposed()
    {
    }

    #endregion

    protected override void OnCommand( ITransport sender, ICommand command )
    {
        if ( Tracer.IsInfoEnabled )
            Tracer.Info( "RECEIVED: " + command );

        Command?.Invoke( sender, command );
    }

    protected override void OnException( ITransport sender, Exception error )
    {
        if ( Tracer.IsErrorEnabled )
            Tracer.Error( "RECEIVED Exception: " + error );

        Exception?.Invoke( sender, error );
    }
}