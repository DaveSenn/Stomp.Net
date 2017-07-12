#region Usings

using System;
using Extend;

#endregion

namespace Stomp.Net.Example.SendReceiveCore
{
    public class Program
    {
        #region Constants

        private const String Destination = "TestQ";
        private const String Host = "127.0.0.1";
        private const String Password = "password";
        private const Int32 Port = 64011;
        private const String User = "admin";

        #endregion

        public static void Main( String[] args )
        {
            // Configure a logger to capture the output of the library
            Tracer.Trace = new ConsoleLogger();

            SendReceiveText();

            Console.WriteLine( "\n\nPress <enter> to exit." );
            Console.ReadLine();
        }

        private static void SendReceiveText()
        {
            // Create a connection factory
            var brokerUri = "tcp://" + Host + ":" + Port;
            //brokerUri = "ssl://" + Host + ":" + Port;

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
                                                     }
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
                        var message = session.CreateTextMessage( RandomValueEx.GetRandomString() );
                        message.Headers["test"] = "test";
                        producer.Send( message );
                        Console.WriteLine( "Message sent\n" );
                    }

                    // Create a message consumer
                    IDestination sourceQueue = session.GetQueue( Destination );
                    using ( var consumer = session.CreateConsumer( sourceQueue ) )
                    {
                        // Wait for a message => blocking call; use consumer.Listener to receive messages as events (none blocking call)
                        var msg = consumer.Receive();
                        if ( msg is ITextMessage )
                        {
                            Console.WriteLine( "Message received" );
                            msg.Acknowledge();
                            foreach ( var key in msg.Headers.Keys )
                                Console.WriteLine( $"\t{msg.Headers[key]}" );
                        }
                        else
                            Console.WriteLine( "Unexpected message type: " + msg.GetType()
                                                                                .Name );
                    }
                }
            }
        }
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