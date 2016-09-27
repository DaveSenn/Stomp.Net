#region Usings

using System;
using System.Collections.Generic;
using Apache.NMS.Stomp.Commands;
using Stomp.Net;

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

        private static readonly Tracked TrackedResponseMarker = new Tracked(  );

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
        
        public override Response ProcessAddConnection( ConnectionInfo info )
        {
            if ( info != null )
                connectionStates.Add( info.ConnectionId, new ConnectionState( info ) );
            return TrackedResponseMarker;
        }

        public override Response ProcessAddConsumer( ConsumerInfo info )
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
            return TrackedResponseMarker;
        }

        public override Response ProcessRemoveConnection( ConnectionId id )
        {
            if ( id != null )
                connectionStates.Remove( id );
            return TrackedResponseMarker;
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
            return TrackedResponseMarker;
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <returns>null if the command is not state tracked.</returns>
        public Tracked Track( ICommand command )
        {
            try
            {
                return (Tracked) command.visit( this );
            }
            catch ( IOException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                throw new IOException( ex.Message );
            }
        }

        public void trackBack( ICommand command )
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