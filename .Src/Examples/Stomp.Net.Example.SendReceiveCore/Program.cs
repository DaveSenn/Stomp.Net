#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace Stomp.Net.Example.SendReceiveCore
{
    public class Program
    {
        private const String Destination = "TestQ";
        private const String Host = "mq";
        private const String Password = "password";
        private const Int32 Port = 61613;
        private const String User = "admin";

        private static IConnection _connection;
        private static List<IMessageConsumer> _consumers;
        private static ISession _session;
        private static List<IDestination> _sourceQueues;

        public static void Main( String[] args )
        {
            // Configure a logger to capture the output of the library
            Tracer.Trace = new ConsoleLogger();
            Tracer.AddCallerInfo = true;

            Connect();

            while ( true )
            {
                var key = Console.ReadKey();
                switch ( key.Key )
                {
                    case ConsoleKey.Enter:
                    case ConsoleKey.Escape:
                        break;
                    case ConsoleKey.C:
                        Console.Clear();
                        break;
                }
            }
        }

        private static void _connection_ExceptionListener( Exception ex )
        {
            _connection.Close();
            _consumers.Clear();
            _sourceQueues.Clear();

            Console.WriteLine( $"Connection error: {ex}" );

            Connect();
        }

        private static void Connect()
        {
            _consumers = new List<IMessageConsumer>();
            _sourceQueues = new List<IDestination>();

            var brokerUri = "tcp://" + Host + ":" + Port;
            var factory = new ConnectionFactory( brokerUri,
                                                 new StompConnectionSettings
                                                 {
                                                     UserName = User,
                                                     Password = Password,
                                                     SkipDestinationNameFormatting = false,
                                                     SetHostHeader = true,
                                                     HostHeaderOverride = null,
                                                     TransportSettings =
                                                     {
                                                         UseInactivityMonitor = true,
                                                         // ReceiveTimeout = 10_000,
                                                         // SendTimeout = 20_000,
                                                         UseLogging = true
                                                     }
                                                     // RequestTimeout = 45.ToSeconds()
                                                 } );

            _connection = factory.CreateConnection();
            _connection.Start();
            _connection.ExceptionListener += _connection_ExceptionListener;
            _session = _connection.CreateSession( AcknowledgementMode.IndividualAcknowledge );

            for ( var i = 0; i < 10; i++ )
            {
                var sourceQueue = _session.GetQueue( Destination + i );
                _sourceQueues.Add( sourceQueue );
                var consumer = _session.CreateConsumer( sourceQueue );
                _consumers.Add( consumer );

                consumer.Listener += x => Console.WriteLine( "Message received" );
            }
        }
    }
}

/*
 public class Program
    {
        public static void Main( String[] args )
        {
            // Configure a logger to capture the output of the library
            Tracer.Trace = new ConsoleLogger();
            Tracer.AddCallerInfo = true;

            SendReceiveText();

            Console.WriteLine( "\n\nPress <enter> to exit." );
            Console.ReadLine();
        }

        private static void SendReceiveText()
        {
            // Create a connection factory
            var brokerUri = "tcp://" + Host + ":" + Port;
            // SSL: brokerUri = "ssl://" + Host + ":" + Port;

            var factory = new ConnectionFactory( brokerUri,
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
                                                     SkipDestinationNameFormatting = false, // Determines whether the destination name formatting should be skipped or not.
                                                     SetHostHeader = true, // Determines whether the host header will be added to messages or not
                                                     HostHeaderOverride = null // Can be used to override the content of the host header
                                                 } );

            // Create connection for both requests and responses
            using ( var connection = factory.CreateConnection() )
            {
                // Open the connection
                connection.Start();

                // Create session for both requests and responses
                using ( var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge ) )
                {
                    // Create a message producer
                    IDestination destinationQueue = session.GetQueue( Destination );
                    using ( var producer = session.CreateProducer( destinationQueue ) )
                    {
                        producer.DeliveryMode = MessageDeliveryMode.Persistent;

                        // Send a message to the destination
                        var message = session.CreateBytesMessage( Encoding.UTF8.GetBytes( "Hello World" ) );
                        message.StompTimeToLive = TimeSpan.FromMinutes( 1 );
                        message.Headers["test"] = "test";
                        producer.Send( message );
                        Console.WriteLine( "\n\nMessage sent\n" );
                    }

                    // Create a message consumer
                    IDestination sourceQueue = session.GetQueue( Destination );
                    using ( var consumer = session.CreateConsumer( sourceQueue ) )
                    {
                        // Wait for a message => blocking call; use consumer.Listener to receive messages as events (none blocking call)
                        var msg = consumer.Receive();

                        var s = Encoding.UTF8.GetString( msg.Content );
                        Console.WriteLine( $"\n\nMessage received: {s} from destination: {msg.FromDestination.PhysicalName}" );

                        msg.Acknowledge();
                        foreach ( var key in msg.Headers.Keys )
                            Console.WriteLine( $"\t{msg.Headers[key]}" );
                    }
                }
            }
        }

        #region Constants

        private const String Destination = "TestQ";
        private const String Host = "mq";
        private const String Password = "password";
        private const String User = "admin";
        private const Int32 Port = 61613;

        #endregion
    }
 */