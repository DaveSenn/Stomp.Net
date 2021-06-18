using System;
using System.Collections.Generic;

namespace Stomp.Net.Example.ReconnectOnConnectionError
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
            _consumers = new();
            _sourceQueues = new();

            var brokerUri = "tcp://" + Host + ":" + Port;
            var factory = new ConnectionFactory( brokerUri,
                                                 new()
                                                 {
                                                     UserName = User,
                                                     Password = Password,
                                                     SkipDestinationNameFormatting = false,
                                                     SetHostHeader = true,
                                                     HostHeaderOverride = null,
                                                     TransportSettings =
                                                     {
                                                         UseInactivityMonitor = true,
                                                         UseLogging = true
                                                     }
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