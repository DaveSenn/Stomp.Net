#region Usings

using System;
using System.Net.Sockets;
using Apache.NMS.Stomp.Transport;
using Extend;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Transport
{
    /// <summary>
    ///     SSL transport factory.
    /// </summary>
    public class SslTransportFactory : TcpTransportFactory
    {
        #region Fields

        /// <summary>
        ///     Stores the STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="SslTransportFactory" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">stompConnectionSettings can not be null.</exception>
        /// <param name="stompConnectionSettings">Some STOMP settings.</param>
        public SslTransportFactory( [NotNull] StompConnectionSettings stompConnectionSettings )
            : base( stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        protected override ITransport CreateTransport( Uri location, Socket socket, IWireFormat wireFormat )
            => new SslTransport( location, socket, wireFormat, _stompConnectionSettings );
    }
}