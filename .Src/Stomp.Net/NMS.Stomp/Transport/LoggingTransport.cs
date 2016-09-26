#region Usings

using System;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.Transport
{
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

        public override void Oneway( Command command )
        {
            Tracer.Info( "SENDING: " + command );
            next.Oneway( command );
        }

        protected override void OnCommand( ITransport sender, Command command )
        {
            Tracer.Info( "RECEIVED: " + command );
            commandHandler( sender, command );
        }

        protected override void OnException( ITransport sender, Exception error )
        {
            Tracer.Error( "RECEIVED Exception: " + error );
            exceptionHandler( sender, error );
        }
    }
}