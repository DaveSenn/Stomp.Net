#region Usings

using System;
using System.Diagnostics;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.State
{
    public class ConnectionState
    {
        #region Fields

        private readonly Atomic<Boolean> _shutdown = new Atomic<Boolean>( false );
        private readonly AtomicDictionary<ConsumerId, ConsumerState> consumers = new AtomicDictionary<ConsumerId, ConsumerState>();

        #endregion

        #region Properties

        public ConsumerState this[ ConsumerId id ]
        {
            get
            {
                ConsumerState consumerState = null;

                consumers.TryGetValue( id, out consumerState );

#if DEBUG
                if ( null == consumerState )
                {
                    // Useful for dignosing missing consumer ids
                    var consumerList = String.Empty;
                    foreach ( ConsumerId consumerId in consumers.Keys )
                        consumerList += consumerId + "\n";

                    Debug.Assert( false,
                                  String.Format( "Consumer '{0}' did not exist in the consumers collection.\n\nConsumers:-\n{1}", id, consumerList ) );
                }
#endif
                return consumerState;
            }
        }

        public ConnectionInfo Info { get; private set; }

        public AtomicCollection<ConsumerId> ConsumerIds => consumers.Keys;

        public AtomicCollection<ConsumerState> ConsumerStates => consumers.Values;

        #endregion

        #region Ctor

        public ConnectionState( ConnectionInfo info )
        {
            Info = info;
        }

        #endregion

        public void addConsumer( ConsumerInfo info )
        {
            checkShutdown();

            var consumerState = new ConsumerState( info );

            if ( consumers.ContainsKey( info.ConsumerId ) )
                consumers[info.ConsumerId] = consumerState;
            else
                consumers.Add( info.ConsumerId, consumerState );
        }

        public ConsumerState removeConsumer( ConsumerId id )
        {
            ConsumerState ret = null;

            if ( consumers.TryGetValue( id, out ret ) )
                consumers.Remove( id );
            return ret;
        }

        public void reset( ConnectionInfo info )
        {
            Info = info;
            _shutdown.Value = false;
        }

        public void shutdown()
        {
            if ( _shutdown.CompareAndSet( false, true ) )
                consumers.Clear();
        }

        public override String ToString() => Info.ToString();

        private void checkShutdown()
        {
            if ( _shutdown.Value )
                throw new ApplicationException( "Disposed" );
        }
    }
}