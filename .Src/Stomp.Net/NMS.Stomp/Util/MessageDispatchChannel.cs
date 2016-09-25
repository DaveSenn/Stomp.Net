

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

        private readonly LinkedList<MessageDispatch> channel = new LinkedList<MessageDispatch>();
        private readonly Mutex mutex = new Mutex();
        private readonly ManualResetEvent waiter = new ManualResetEvent( false );
        private Boolean closed;
        private Boolean running;

        #endregion

        #region Properties

        public Object SyncRoot
        {
            get { return mutex; }
        }

        public Boolean Closed
        {
            get
            {
                lock ( mutex )
                    return closed;
            }

            set
            {
                lock ( mutex )
                    closed = value;
            }
        }

        public Boolean Running
        {
            get
            {
                lock ( mutex )
                    return running;
            }

            set
            {
                lock ( mutex )
                    running = value;
            }
        }

        public Boolean Empty
        {
            get
            {
                lock ( mutex )
                    return channel.Count == 0;
            }
        }

        public Int64 Count
        {
            get
            {
                lock ( mutex )
                    return channel.Count;
            }
        }

        #endregion

        #region Ctor

        #endregion

        public void Clear()
        {
            lock ( mutex )
                channel.Clear();
        }

        public void Close()
        {
            lock ( mutex )
            {
                if ( !Closed )
                {
                    running = false;
                    closed = true;
                }

                waiter.Set();
            }
        }

        public MessageDispatch Dequeue( TimeSpan timeout )
        {
            MessageDispatch result = null;

            mutex.WaitOne();

            // Wait until the channel is ready to deliver messages.
            if ( timeout != TimeSpan.Zero && !Closed && ( Empty || !Running ) )
            {
                // This isn't the greatest way to do this but to work on the
                // .NETCF its the only solution I could find so far.  This
                // code will only really work for one Thread using the event
                // channel to wait as all waiters are going to drop out of
                // here regardless of the fact that only one message could
                // be on the Queue.  
                waiter.Reset();
                mutex.ReleaseMutex();
                waiter.WaitOne( (Int32) timeout.TotalMilliseconds, false );
                mutex.WaitOne();
            }

            if ( !Closed && Running && !Empty )
                result = DequeueNoWait();

            mutex.ReleaseMutex();

            return result;
        }

        public MessageDispatch DequeueNoWait()
        {
            MessageDispatch result = null;

            lock ( mutex )
            {
                if ( Closed || !Running || Empty )
                    return null;

                result = channel.First.Value;
                channel.RemoveFirst();
            }

            return result;
        }

        public void Enqueue( MessageDispatch dispatch )
        {
            lock ( mutex )
            {
                channel.AddLast( dispatch );
                waiter.Set();
            }
        }

        public void EnqueueFirst( MessageDispatch dispatch )
        {
            lock ( mutex )
            {
                channel.AddFirst( dispatch );
                waiter.Set();
            }
        }

        public MessageDispatch Peek()
        {
            lock ( mutex )
            {
                if ( Closed || !Running || Empty )
                    return null;

                return channel.First.Value;
            }
        }

        public MessageDispatch[] RemoveAll()
        {
            MessageDispatch[] result;

            lock ( mutex )
            {
                result = new MessageDispatch[Count];
                channel.CopyTo( result, 0 );
                channel.Clear();
            }

            return result;
        }

        public void Start()
        {
            lock ( mutex )
                if ( !Closed )
                {
                    running = true;
                    waiter.Reset();
                }
        }

        public void Stop()
        {
            lock ( mutex )
            {
                running = false;
                waiter.Set();
            }
        }
    }
}