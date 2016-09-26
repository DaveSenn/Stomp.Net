

#region Usings

using System;
using System.Collections;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.Threads;
using Apache.NMS.Stomp.Util;

#endregion

namespace Apache.NMS.Stomp
{
    public class SessionExecutor : Task
    {
        #region Fields

        private readonly IDictionary consumers;
        private readonly MessageDispatchChannel messageQueue = new MessageDispatchChannel();

        private readonly Session session;
        private TaskRunner taskRunner;

        #endregion

        #region Properties

        public MessageDispatch[] UnconsumedMessages
        {
            get { return messageQueue.RemoveAll(); }
        }

        public Boolean HasUncomsumedMessages
        {
            get { return !messageQueue.Closed && messageQueue.Running && !messageQueue.Empty; }
        }

        public Boolean Running
        {
            get { return messageQueue.Running; }
        }

        public Boolean Empty
        {
            get { return messageQueue.Empty; }
        }

        #endregion

        #region Ctor

        public SessionExecutor( Session session, IDictionary consumers )
        {
            this.session = session;
            this.consumers = consumers;
        }

        #endregion

        public Boolean Iterate()
        {
            try
            {
                lock ( consumers.SyncRoot )
                    foreach ( MessageConsumer consumer in consumers.Values )
                        if ( consumer.Iterate() )
                            return true;

                // No messages left queued on the listeners.. so now dispatch messages
                // queued on the session
                var message = messageQueue.DequeueNoWait();

                if ( message != null )
                {
                    Dispatch( message );
                    return !messageQueue.Empty;
                }

                return false;
            }
            catch ( Exception ex )
            {
                Tracer.DebugFormat( "Caught Exception While Dispatching: {0}", ex.Message );
                session.Connection.OnSessionException( session, ex );
            }

            return true;
        }

        public void Clear() => messageQueue.Clear();

        public void ClearMessagesInProgress() => messageQueue.Clear();

        public void Close() => messageQueue.Close();

        public void Dispatch( MessageDispatch dispatch )
        {
            try
            {
                MessageConsumer consumer = null;

                lock ( consumers.SyncRoot )
                    if ( consumers.Contains( dispatch.ConsumerId ) )
                        consumer = consumers[dispatch.ConsumerId] as MessageConsumer;

                // If the consumer is not available, just ignore the message.
                // Otherwise, dispatch the message to the consumer.
                if ( consumer != null )
                    consumer.Dispatch( dispatch );
            }
            catch ( Exception ex )
            {
                Tracer.DebugFormat( "Caught Exception While Dispatching: {0}", ex.Message );
            }
        }

        public void Execute( MessageDispatch dispatch )
        {
            // Add the data to the queue.
            messageQueue.Enqueue( dispatch );
            Wakeup();
        }

        public void ExecuteFirst( MessageDispatch dispatch )
        {
            // Add the data to the queue.
            messageQueue.EnqueueFirst( dispatch );
            Wakeup();
        }

        public void Start()
        {
            if ( !messageQueue.Running )
            {
                messageQueue.Start();

                if ( HasUncomsumedMessages )
                    Wakeup();
            }
        }

        public void Stop()
        {
            if ( messageQueue.Running )
            {
                messageQueue.Stop();
                var taskRunner = this.taskRunner;

                if ( taskRunner != null )
                {
                    this.taskRunner = null;
                    taskRunner.Shutdown();
                }
            }
        }

        public void Wakeup()
        {
            var taskRunner = this.taskRunner;

            lock ( messageQueue.SyncRoot )
            {
                if ( this.taskRunner == null )
                    this.taskRunner = new DedicatedTaskRunner( this );

                taskRunner = this.taskRunner;
            }

            taskRunner.Wakeup();
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
            }
        }
    }
}