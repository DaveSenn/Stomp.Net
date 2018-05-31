#region Usings

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extend;

#endregion

namespace Stomp.Net.Example.SelectorsCore
{
    public class Program
    {
        #region Constants

        private const String Host = "host";

        private const Int32 NoOfMessages = 30;

        private const String Password = "password";

        private const Int32 Port = 63617;
        //private const Int32 Port = 61613;

        private const String QueueName = "TestQ";
        private const String User = "admin";

        private static readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();

        #endregion

        public static void Main( String[] args )
        {
            // Configure a logger to capture the output of the library
            Tracer.Trace = new ConsoleLogger();

            try
            {
                using ( var subscriber = new Subscriber() )
                {
                    SendMessages();

                    Console.WriteLine( $" [{Thread.CurrentThread.ManagedThreadId}] Start receiving messages." );

                    subscriber.Start();

                    Console.WriteLine( _resetEvent.Wait( 1.ToMinutes() ) ? "All messages received" : "Timeout :(" );
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Error: {ex}" );
            }

            Console.WriteLine( "Press <enter> to exit." );
            Console.ReadLine();
        }

        private static ConnectionFactory GetConnectionFactory()
        {
            // Create a connection factory
            var brokerUri = "tcp://" + Host + ":" + Port;

            return new ConnectionFactory( brokerUri,
                                          new StompConnectionSettings
                                          {
                                              UserName = User,
                                              Password = Password,
                                              TransportSettings =
                                              {
                                                  SslSettings =
                                                  {
                                                      ServerName = "",
                                                      ClientCertSubject = "",
                                                      KeyStoreName = "My",
                                                      KeyStoreLocation = "LocalMachine"
                                                  }
                                              },
                                              SkipDesinationNameFormatting = false, // Determines whether the destination name formatting should be skipped or not.
                                              SetHostHeader = true, // Determines whether the host header will be added to messages or not
                                              HostHeaderOverride = null // Can be used to override the content of the host header
                                          } );
        }

        private static void SendMessages()
        {
            var factory = GetConnectionFactory();

            // Create connection for both requests and responses
            using ( var connection = factory.CreateConnection() )
            {
                // Open the connection
                connection.Start();

                // Create session for both requests and responses
                using ( var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge ) )
                {
                    // Create a message producer
                    IDestination destinationQueue = session.GetQueue( QueueName );
                    using ( var producer = session.CreateProducer( destinationQueue ) )
                    {
                        producer.DeliveryMode = MessageDeliveryMode.Persistent;

                        for ( var i = 0; i < NoOfMessages; i++ )
                        {
                            // Send a message to the destination
                            var message = session.CreateBytesMessage( Encoding.UTF8.GetBytes( $"Hello World {i}" ) );
                            message.StompTimeToLive = TimeSpan.FromMinutes( 1 );
                            message.Headers["test"] = $"test {i}";
                            producer.Send( message );

                            Console.WriteLine( $"Message sent {i}" );
                        }
                    }
                }
            }
        }

        #region Nested Types

        private class Subscriber : IDisposable
        {
            #region Fields

            private readonly Object _sync = new Object();
            private IConnection _connection;
            private IMessageConsumer _consumer;

            private Int32 _noOfreceivedMessages;
            private ISession _session;

            #endregion

            #region IDisposable

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                _connection?.Dispose();
                _session?.Dispose();
                _consumer?.Dispose();
            }

            #endregion

            public void Start()
            {
                var factory = GetConnectionFactory();

                // Create connection for both requests and responses
                _connection = factory.CreateConnection();

                // Open the connection
                _connection.Start();

                // Create session for both requests and responses
                _session = _connection.CreateSession( AcknowledgementMode.IndividualAcknowledge );

                // Create a message consumer
                IDestination sourceQueue = _session.GetQueue( QueueName );
                _consumer = _session.CreateConsumer( sourceQueue );

                _consumer.Listener += async message =>
                {
                    await Task.Delay( 500 );

                    var content = Encoding.UTF8.GetString( message.Content );
                    Console.WriteLine( $" [{Thread.CurrentThread.ManagedThreadId}] {content}" );

                    message.Acknowledge();

                    lock ( _sync )
                    {
                        _noOfreceivedMessages++;

                        if ( _noOfreceivedMessages >= NoOfMessages )
                            _resetEvent.Set();
                    }
                };
            }
        }

        #endregion
    }

    /// <summary>
    ///     Console logger for Stomp.Net
    /// </summary>
    public class ConsoleLogger : ITrace
    {
        #region Implementation of ITrace

        /// <summary>
        ///     Writes a message on the error level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error( String message )
            => Console.WriteLine( $"[Error]\t\t{message}" );

        /// <summary>
        ///     Writes a message on the fatal level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Fatal( String message )
            => Console.WriteLine( $"[Fatal]\t\t{message}" );

        /// <summary>
        ///     Writes a message on the info level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info( String message )
            => Console.WriteLine( $"[Info]\t\t{message}" );

        /// <summary>
        ///     Writes a message on the warn level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warn( String message )
            => Console.WriteLine( $"[Warn]\t\t{message}" );

        #endregion
    }
}