#region Usings

using System;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp.Transport
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

        public override void Oneway( ICommand command )
        {
            Tracer.Info( "SENDING: " + command );
            Next.Oneway( command );
        }

        protected override void OnCommand( ITransport sender, ICommand command )
        {
            Tracer.Info( "RECEIVED: " + command );
            Command( sender, command );
        }

        protected override void OnException( ITransport sender, Exception error )
        {
            Tracer.Error( "RECEIVED Exception: " + error );
            Exception( sender, error );
        }
    }
}