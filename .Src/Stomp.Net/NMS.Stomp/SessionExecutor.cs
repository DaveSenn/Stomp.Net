#region Usings

using System;
using System.Collections.Concurrent;
using System.Linq;
using Stomp.Net.Messaging;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Stomp.Threads;

#endregion

namespace Stomp.Net.Stomp
{
    public class SessionExecutor : ITask
    {
        #region Fields

        private readonly ConcurrentDictionary<ConsumerId, MessageConsumer> _consumers;
        private readonly MessageDispatchChannel _messageQueue = new MessageDispatchChannel();

        private readonly Session _session;
        private readonly Object _syncRoot = new Object();
        private ITaskRunner _taskRunner;

        #endregion

        #region Properties

        public MessageDispatch[] UnconsumedMessages => _messageQueue.EnqueueAll();

        private Boolean HasUncomsumedMessages => _messageQueue.Started && _messageQueue.HasMessages;

        public Boolean Running => _messageQueue.Started;

        #endregion

        #region Ctor

        public SessionExecutor( Session session, ConcurrentDictionary<ConsumerId, MessageConsumer> consumers )
        {
            _session = session;
            _consumers = consumers;
        }

        #endregion

        public Boolean Iterate()
        {
            try
            {
                if ( _consumers.Any( consumer => consumer.Value.Iterate() ) )
                    return true;

                // No messages left queued on the listeners.. so now dispatch messages
                // queued on the session
                var message = _messageQueue.DequeueNoWait();

                if ( message == null )
                    return false;

                Dispatch( message );
                return _messageQueue.HasMessages;
            }
            catch ( Exception ex )
            {
                Tracer.Warn( $"Caught Exception While Dispatching: {ex}" );
                _session.Connection.OnSessionException( _session, ex );
            }

            return true;
        }

        public void ClearMessagesInProgress() => _messageQueue.Clear();

        public void Execute( MessageDispatch dispatch )
        {
            // Add the data to the queue.
            _messageQueue.Enqueue( dispatch );
            Wakeup();
        }

        public void ExecuteFirst( MessageDispatch dispatch )
        {
            // Add the data to the queue.
            _messageQueue.Enqueue( dispatch, true );
            Wakeup();
        }

        public void Start()
        {
            if ( _messageQueue.Started )
                return;
            _messageQueue.Start();

            if ( HasUncomsumedMessages )
                Wakeup();
        }

        public void Stop()
        {
            if ( !_messageQueue.Started )
                return;
            _messageQueue.Stop();
            var taskRunner = _taskRunner;

            if ( taskRunner == null )
                return;
            _taskRunner = null;
            taskRunner.Shutdown();
        }

        public void Wakeup()
        {
            ITaskRunner taskRunner;

            lock ( _syncRoot )
            {
                if ( _taskRunner == null )
                    _taskRunner = new DedicatedTaskRunner( this );

                taskRunner = _taskRunner;
            }

            taskRunner.Wakeup();
        }

        private void Clear()
            => _messageQueue.Clear();

        private void Close()
            => _messageQueue.Stop();

        private void Dispatch( MessageDispatch dispatch )
        {
            try
            {
                // If the consumer is not available, just ignore the message.
                // Otherwise, dispatch the message to the consumer.
                if ( _consumers.TryGetValue( dispatch.ConsumerId, out MessageConsumer consumer ) )
                    consumer.Dispatch( dispatch );
            }
            catch ( Exception ex )
            {
                Tracer.Warn( $"Caught Exception While Dispatching: {ex}" );
            }
        }

        ~SessionExecutor()
        {
            try
            {
                Stop();
                Close();
                Clear();
            }
            catch
            {
                // ignored
            }
        }
    }
}