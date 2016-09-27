#region Usings

using System;
using System.Collections.Generic;
using Apache.NMS.Util;
using Extend;
using JetBrains.Annotations;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport.Failover
{
    public class FailoverTransportFactory : ITransportFactory
    {
        #region Fields

        /// <summary>
        ///     The STOMP connection settings.
        /// </summary>
        private readonly StompConnectionSettings _stompConnectionSettings;

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="FailoverTransportFactory" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">stompConnectionSettings can not be null.</exception>
        /// <param name="stompConnectionSettings">Some STOMP settings.</param>
        public FailoverTransportFactory( [NotNull] StompConnectionSettings stompConnectionSettings )
        {
            stompConnectionSettings.ThrowIfNull( nameof( stompConnectionSettings ) );

            _stompConnectionSettings = stompConnectionSettings;
        }

        #endregion

        public ITransport CompositeConnect( Uri location )
            => CreateTransport( URISupport.ParseComposite( location ) );

        public ITransport CreateTransport( Uri location ) => doConnect( location );

        /// <summary>
        /// </summary>
        /// <param name="compositData"></param>
        /// <returns></returns>
        public ITransport CreateTransport( URISupport.CompositeData compositData )
        {
            var options = compositData.Parameters;
            var transport = CreateTransport( options );
            transport.Add( compositData.Components );
            return transport;
        }

        public FailoverTransport CreateTransport( Dictionary<String, String> parameters )
        {
            var transport = new FailoverTransport( _stompConnectionSettings );
            URISupport.SetProperties( transport, parameters, "" );
            return transport;
        }

        private ITransport doConnect( Uri location )
        {
            var transport = CreateTransport( URISupport.ParseComposite( location ) );
            transport = new MutexTransport( transport );
            transport = new ResponseCorrelator( transport );
            return transport;
        }
    }
}