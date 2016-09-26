/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System;
using Apache.NMS.Stomp.Transport.Failover;
using Apache.NMS.Stomp.Transport.Tcp;
using Extend;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    public class TransportFactory
    {
        #region Public Members

        /// <summary>
        ///     Creates a normal transport.
        /// </summary>
        /// <param name="location"></param>
        /// <returns>the transport</returns>
        public static ITransport CreateTransport(Uri location)
            => CreateTransportFactory(location).CreateTransport(location);

        public static ITransport CompositeConnect(Uri location)
            => CreateTransportFactory(location).CompositeConnect(location);

        #endregion

        #region Private Members

        /// <summary>
        ///     Create a transport factory for the scheme.
        ///     If we do not support the transport protocol, an NMSConnectionException will be thrown.
        /// </summary>
        /// <param name="location">An URI.</param>
        /// <returns>Returns a <see cref="ITransportFactory" />.</returns>
        private static ITransportFactory CreateTransportFactory( Uri location )
        {
            if ( location.Scheme.IsEmpty() )
                throw new NMSConnectionException( $"Transport scheme invalid: [{location}]" );

            ITransportFactory factory;

            try
            {
                switch ( location.Scheme.ToLower() )
                {
                    case "failover":
                        factory = new FailoverTransportFactory();
                        break;
                    case "tcp":
                        factory = new TcpTransportFactory();
                        break;
                    case "ssl":
                        factory = new SslTransportFactory();
                        break;
                    default:
                        throw new NMSConnectionException( $"The transport {location.Scheme} is not supported." );
                }
            }
            catch ( NMSConnectionException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                throw new NMSConnectionException( "Error creating transport.", ex );
            }

            if ( null == factory )
                throw new NMSConnectionException( "Unable to create a transport." );

            return factory;
        }

        #endregion
    }
}