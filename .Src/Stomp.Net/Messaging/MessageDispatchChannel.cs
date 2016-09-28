#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Extend;
using JetBrains.Annotations;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Messaging
{
    /// <summary>
    ///     Class representing a message dispatcher channel.
    /// </summary>
    public class MessageDispatchChannel
    {
        #region Fields

        /// <summary>
        ///     The message queue.
        /// </summary>
        private readonly List<MessageDispatch> _channel = new List<MessageDispatch>();

        /// <summary>
        ///     Object used to synchronize the access to the message queue.
        /// </summary>
        private readonly Object _syncRoot = new Object();

        /// <summary>
        ///     Stores whether the channel is running or not.
        /// </summary>
        private Boolean _running = true;

        #endregion

        #region Properties

        /// <summary>
        ///     Stopped
        /// </summary>
        public Boolean Stopped
        {
            get
            {
                lock ( _syncRoot )
                    return !_running;
            }
        }

        public Boolean Started
        {
            get
            {
                lock ( _syncRoot )
                    return _running;
            }
        }

        /// <summary>
        ///     Gets whether any messages are available or not.
        /// </summary>
        public Boolean HasMessages
        {
            get
            {
                lock ( _syncRoot )
                    return _channel.Any();
            }
        }

        #endregion

        #region Public Members

        /// <summary>
        ///     Tries to dequeue a message within the specified timeout.
        /// </summary>
        /// <param name="timeout">The timeout, smaller than <see cref="TimeSpan.Zero" /> mean infinity.</param>
        /// <returns>Returns the dequeued message, or null in case of a timeout.</returns>
        public MessageDispatch Dequeue( TimeSpan timeout )
        {
            // Calculate the deadline
            DateTime deadline;
            if ( timeout > TimeSpan.Zero )
                deadline = DateTime.Now + timeout;
            else
                deadline = DateTime.MaxValue;

            while ( true )
            {
                // Check if deadline is reached
                if ( DateTime.Now > deadline )
                    return null;

                // Try to dequeue a message
                var dispatch = DequeueNoWait();
                if ( dispatch != null )
                    return dispatch;

                // No message was available.
                // TODO: Check if this is a good idea?
                Thread.Sleep( 10 );
            }
        }

        /// <summary>
        ///     Gets the first message in the message queue.
        /// </summary>
        /// <returns>Returns the first message.</returns>
        public MessageDispatch DequeueNoWait()
        {
            lock ( _syncRoot )
            {
                if ( !_running || _channel.NotAny() )
                    return null;

                var result = _channel.First();
                _channel.Remove( result );
                return result;
            }
        }

        /// <summary>
        ///     Adds a message to the queue.
        /// </summary>
        /// <param name="dispatch">The message to add.</param>
        /// <param name="addToTop">A value indicating whether the message will be inserted at the top or not.</param>
        public void Enqueue( MessageDispatch dispatch, Boolean addToTop = false )
        {
            lock ( _syncRoot )
                if ( addToTop )
                    _channel.Insert( 0, dispatch );
                else
                    _channel.Add( dispatch );
        }

        /// <summary>
        ///     Clears the queued messages.
        /// </summary>
        public void Clear()
        {
            lock ( _syncRoot )
                _channel.Clear();
        }

        /// <summary>
        ///     Gets all messages from the queue.
        /// </summary>
        /// <returns>Returns the messages.</returns>
        [MustUseReturnValue]
        public MessageDispatch[] EnqueueAll()
        {
            MessageDispatch[] result;

            lock ( _syncRoot )
            {
                result = new MessageDispatch[_channel.Count];
                _channel.CopyTo( result, 0 );
                _channel.Clear();
            }

            return result;
        }

        /// <summary>
        ///     Starts the channel.
        /// </summary>
        public void Start()
        {
            lock ( _syncRoot )
                _running = true;
        }

        /// <summary>
        ///     Stops the channel.
        /// </summary>
        public void Stop()
        {
            lock ( _syncRoot )
                _running = false;
        }

        #endregion
    }
}