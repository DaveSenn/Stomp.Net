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
using System.Collections.Generic;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.Transport;

#endregion

namespace Apache.NMS.Stomp.State
{
    /// <summary>
    ///     Tracks the state of a connection so a newly established transport can be
    ///     re-initialized to the state that was tracked.
    /// </summary>
    public class ConnectionStateTracker : CommandVisitorAdapter
    {
        #region Constants

        private static readonly Tracked TRACKED_RESPONSE_MARKER = new Tracked( null );

        #endregion

        #region Fields

        protected Dictionary<ConnectionId, ConnectionState> connectionStates = new Dictionary<ConnectionId, ConnectionState>();

        #endregion

        #region Properties

        public Boolean RestoreConsumers { get; set; } = true;

        #endregion

        public void DoRestore( ITransport transport )
        {
            // Restore the connections.
            foreach ( var connectionState in connectionStates.Values )
            {
                transport.Oneway( connectionState.Info );

                if ( RestoreConsumers )
                    DoRestoreConsumers( transport, connectionState );
            }
        }

        public override Response processAddConnection( ConnectionInfo info )
        {
            if ( info != null )
                connectionStates.Add( info.ConnectionId, new ConnectionState( info ) );
            return TRACKED_RESPONSE_MARKER;
        }

        public override Response processAddConsumer( ConsumerInfo info )
        {
            if ( info != null )
            {
                var sessionId = info.ConsumerId.ParentId;
                if ( sessionId != null )
                {
                    var connectionId = sessionId.ParentId;
                    if ( connectionId != null )
                    {
                        ConnectionState cs = null;

                        if ( connectionStates.TryGetValue( connectionId, out cs ) )
                            cs.addConsumer( info );
                    }
                }
            }
            return TRACKED_RESPONSE_MARKER;
        }

        public override Response processRemoveConnection( ConnectionId id )
        {
            if ( id != null )
                connectionStates.Remove( id );
            return TRACKED_RESPONSE_MARKER;
        }

        public override Response processRemoveConsumer( ConsumerId id )
        {
            if ( id != null )
            {
                var sessionId = id.ParentId;
                if ( sessionId != null )
                {
                    var connectionId = sessionId.ParentId;
                    if ( connectionId != null )
                    {
                        ConnectionState cs = null;

                        if ( connectionStates.TryGetValue( connectionId, out cs ) )
                            cs.removeConsumer( id );
                    }
                }
            }
            return TRACKED_RESPONSE_MARKER;
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <returns>null if the command is not state tracked.</returns>
        public Tracked track( Command command )
        {
            try
            {
                return (Tracked) command.visit( this );
            }
            catch ( IOException e )
            {
                throw e;
            }
            catch ( Exception e )
            {
                throw new IOException( e.Message );
            }
        }

        public void trackBack( Command command )
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="connectionState"></param>
        protected void DoRestoreConsumers( ITransport transport, ConnectionState connectionState )
        {
            // Restore the session's consumers
            foreach ( ConsumerState consumerState in connectionState.ConsumerStates )
                transport.Oneway( consumerState.Info );
        }
    }
}