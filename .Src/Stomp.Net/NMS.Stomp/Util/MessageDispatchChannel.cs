#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.Util
{
    public class MessageDispatchChannel
    {
        #region Fields

        private readonly LinkedList<MessageDispatch> _channel = new LinkedList<MessageDispatch>();
        private readonly Mutex _mutex = new Mutex();
        private readonly ManualResetEvent _waiter = new ManualResetEvent( false );
        private Boolean _closed;
        private Boolean _running;

        #endregion

        #region Properties

        public Object SyncRoot => _mutex;

        public Boolean Closed
        {
            get
            {
                lock ( _mutex )
                    return _closed;
            }

            set
            {
                lock ( _mutex )
                    _closed = value;
            }
        }

        public Boolean Running
        {
            get
            {
                lock ( _mutex )
                    return _running;
            }

            set
            {
                lock ( _mutex )
                    _running = value;
            }
        }

        public Boolean Empty
        {
            get
            {
                lock ( _mutex )
                    return _channel.Count == 0;
            }
        }

        public Int64 Count
        {
            get
            {
                lock ( _mutex )
                    return _channel.Count;
            }
        }

        #endregion

        public void Clear()
        {
            lock ( _mutex )
                _channel.Clear();
        }

        public void Close()
        {
            lock ( _mutex )
            {
                if ( !Closed )
                {
                    _running = false;
                    _closed = true;
                }

                _waiter.Set();
            }
        }

        public MessageDispatch Dequeue( TimeSpan timeout )
        {
            MessageDispatch result = null;

            _mutex.WaitOne();

            // Wait until the channel is ready to deliver messages.
            if ( timeout != TimeSpan.Zero && !Closed && ( Empty || !Running ) )
            {
                // This isn't the greatest way to do this but to work on the
                // .NETCF its the only solution I could find so far.  This
                // code will only really work for one Thread using the event
                // channel to wait as all waiters are going to drop out of
                // here regardless of the fact that only one message could
                // be on the Queue.  
                _waiter.Reset();
                _mutex.ReleaseMutex();
                _waiter.WaitOne( (Int32) timeout.TotalMilliseconds, false );
                _mutex.WaitOne();
            }

            if ( !Closed && Running && !Empty )
                result = DequeueNoWait();

            _mutex.ReleaseMutex();

            return result;
        }

        public MessageDispatch DequeueNoWait()
        {
            MessageDispatch result;

            lock ( _mutex )
            {
                if ( Closed || !Running || Empty )
                    return null;

                result = _channel.First.Value;
                _channel.RemoveFirst();
            }

            return result;
        }

        public void Enqueue( MessageDispatch dispatch )
        {
            lock ( _mutex )
            {
                _channel.AddLast( dispatch );
                _waiter.Set();
            }
        }

        public void EnqueueFirst( MessageDispatch dispatch )
        {
            lock ( _mutex )
            {
                _channel.AddFirst( dispatch );
                _waiter.Set();
            }
        }

        public MessageDispatch Peek()
        {
            lock ( _mutex )
            {
                if ( Closed || !Running || Empty )
                    return null;

                return _channel.First.Value;
            }
        }

        public MessageDispatch[] RemoveAll()
        {
            MessageDispatch[] result;

            lock ( _mutex )
            {
                result = new MessageDispatch[Count];
                _channel.CopyTo( result, 0 );
                _channel.Clear();
            }

            return result;
        }

        public void Start()
        {
            lock ( _mutex )
                if ( !Closed )
                {
                    _running = true;
                    _waiter.Reset();
                }
        }

        public void Stop()
        {
            lock ( _mutex )
            {
                _running = false;
                _waiter.Set();
            }
        }
    }
}