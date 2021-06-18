#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Extend;

#endregion

namespace Stomp.Net.Example.SelectorsCore
{
    public class Program
    {
        public static void Main( String[] args )
        {
            // Configure a logger to capture the output of the library
            Tracer.Trace = new ConsoleLogger();

            try
            {
                // Send some messages for each selector
                Selectors
                    .ForEach( x => Send( x, 20 ) );

                // Receive the messages
                var consumers = Selectors
                                .Select( Receive )
                                .ToList();

                // Wait before disposing the consumers
                Thread.Sleep( 10000 );
                consumers.ForEach( x => x.Dispose() );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Error: {ex}" );
            }

            Console.WriteLine( "Press <enter> to exit." );
            Console.ReadLine();
        }

        private static Consumer Receive( String selectorValue )
            => new(SelectorKey, selectorValue);

        private static void Send( String selectorValue, Int32 messageCount )
        {
            // Create a connection factory
            var brokerUri = "tcp://" + Host + ":" + Port;
            var factory = new ConnectionFactory( brokerUri,
                                                 new()
                                                 {
                                                     UserName = User,
                                                     Password = Password,
                                                     TransportSettings =
                                                     {
                                                         UseInactivityMonitor = true
                                                     }
                                                 } );

            // Create connection for both requests and responses
            using var connection = factory.CreateConnection();
            // Open the connection
            connection.Start();

            // Create session for both requests and responses
            using var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge );
            // Create a message producer
            IDestination destinationQueue = session.GetQueue( QueueName );
            using var producer = session.CreateProducer( destinationQueue );
            producer.DeliveryMode = MessageDeliveryMode.NonPersistent;

            // Send {messageCount} messages with the given selector
            for ( var i = 0; i < messageCount; i++ )
            {
                var message = session.CreateBytesMessage( Encoding.UTF8.GetBytes( $"{selectorValue} {i,0:000} => {RandomValueEx.GetRandomString()}" ) );
                // Set the selector value in the message header
                message.Headers[SelectorKey] = selectorValue;

                // Send the message
                producer.Send( message );
            }
        }

        #region Nested Types

        private class Consumer : Disposable
        {
            #region Fields

            private Boolean _running = true;

            #endregion

            #region Ctor

            public Consumer( String selectorKey, String selector )
            {
                Create( selectorKey, selector );
            }

            #endregion

            #region Override of Disposable

            /// <summary>
            ///     Method invoked when the instance gets disposed.
            /// </summary>
            protected override void Disposed()
                => _running = false;

            #endregion

            #region Private Members

            private void Create( String selectorKey, String selector )
                => new Thread( () =>
                {
                    // Create a connection factory
                    var brokerUri = "tcp://" + Host + ":" + Port;
                    var factory = new ConnectionFactory( brokerUri,
                                                         new()
                                                         {
                                                             UserName = User,
                                                             Password = Password,
                                                             TransportSettings =
                                                             {
                                                                 UseInactivityMonitor = true
                                                             }
                                                         } );

                    // Create connection for both requests and responses
                    using var connection = factory.CreateConnection();
                    connection.Start();

                    // Create session for both requests and responses
                    using var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge );
                    var selectorString = $"{selectorKey} = '{selector}'";
                    Console.WriteLine( $"Create consumer with selector {selectorString}" );

                    // Create a message consumer with the given selector
                    IDestination responseQ = session.GetQueue( QueueName );
                    using var consumer = session.CreateConsumer( responseQ, selectorString );
                    // Start receiving messages => none blocking call
                    consumer.Listener += x =>
                    {
                        var content = Encoding.UTF8.GetString( x.Content );
                        Console.WriteLine( $"{selector}\t => {x.Headers[selectorKey]} => {content}" );

                        x.Acknowledge();
                    };

                    // Keep the thread alive
                    while ( _running )
                        Thread.Sleep( 1000 );
                } ).Start();

            #endregion
        }

        #endregion

        #region Constants

        private const String Host = "host";

        private const String Password = "password";

        private const Int32 Port = 63617;
        //private const Int32 Port = 61613;

        private const String QueueName = "TestQ";
        private const String SelectorKey = "selectorProp";
        private const String User = "admin";
        private static readonly List<String> Selectors = new() { "s1", "s2", "s3" };

        #endregion
    }

    /// <summary>
    ///     Console logger for Stomp.Net
    /// </summary>
    public class ConsoleLogger : ITrace
    {
        #region Implementation of ITrace

        /// <summary>
        ///     Gets a value indicating whether the error level is enabled or not.
        /// </summary>
        public Boolean IsErrorEnabled => true;

        /// <summary>
        ///     Gets a value indicating whether the warn level is enabled or not.
        /// </summary>
        public Boolean IsWarnEnabled => true;

        /// <summary>
        ///     Gets a value indicating whether the info level is enabled or not.
        /// </summary>
        public Boolean IsInfoEnabled => true;

        /// <summary>
        ///     Gets a value indicating whether the fatal level is enabled or not.
        /// </summary>
        public Boolean IsFatalEnabled => true;

        /// <summary>
        ///     Gets a value indicating whether the debug level is enabled or not.
        /// </summary>
        public Boolean IsDebugEnabled => true;

        /// <summary>
        ///     Writes a message on the error level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error( String message )
            => Console.WriteLine( $"[Error]\t{message}" );

        /// <summary>
        ///     Writes a message on the fatal level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Fatal( String message )
            => Console.WriteLine( $"[Fatal]\t{message}" );

        /// <summary>
        ///     Writes a message on the info level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info( String message )
            => Console.WriteLine( $"[Info]\t{message}" );

        /// <summary>
        ///     Writes a message on the debug level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug( String message )
            => Console.WriteLine( $"[Debug]\t{message}" );

        /// <summary>
        ///     Writes a message on the warn level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warn( String message )
            => Console.WriteLine( $"[Warn]\t{message}" );

        #endregion
    }
}