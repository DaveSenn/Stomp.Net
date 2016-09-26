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

#endregion

namespace Apache.NMS.Stomp.Transport
{
    public class TransportFactory
    {
        public static ITransport AsyncCompositeConnect( Uri location, SetTransport setTransport )
        {
            var tf = CreateTransportFactory( location );
            return tf.CompositeConnect( location, setTransport );
        }

        public static ITransport CompositeConnect( Uri location )
        {
            var tf = CreateTransportFactory( location );
            return tf.CompositeConnect( location );
        }

        /// <summary>
        ///     Creates a normal transport.
        /// </summary>
        /// <param name="location"></param>
        /// <returns>the transport</returns>
        public static ITransport CreateTransport( Uri location )
        {
            var tf = CreateTransportFactory( location );
            return tf.CreateTransport( location );
        }

        public static void HandleException( Exception ex )
            => OnException?.Invoke( ex );

        public static event ExceptionListener OnException;

        /// <summary>
        ///     Create a transport factory for the scheme.  If we do not support the transport protocol,
        ///     an NMSConnectionException will be thrown.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private static ITransportFactory CreateTransportFactory( Uri location )
        {
            var scheme = location.Scheme;

            if ( String.IsNullOrEmpty( scheme ) )
                throw new NMSConnectionException( String.Format( "Transport scheme invalid: [{0}]", location ) );

            ITransportFactory factory = null;

            try
            {
                switch ( scheme.ToLower() )
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
                        throw new NMSConnectionException( String.Format( "The transport {0} is not supported.", scheme ) );
                }
            }
            catch ( NMSConnectionException )
            {
                throw;
            }
            catch
            {
                throw new NMSConnectionException( "Error creating transport." );
            }

            if ( null == factory )
                throw new NMSConnectionException( "Unable to create a transport." );

            return factory;
        }
    }
}