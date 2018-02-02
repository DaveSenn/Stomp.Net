#region Usings

using System;
using System.Text;
using Extend;

#endregion

namespace Stomp.Net.Example.SendReceiveCore
{
    public class Program
    {
        #region Constants

        private const String Destination = "TestQ";
        private const String Host = "hostName";
        private const String Password = "password";

        private const Int32 Port = 61613;

        //private const Int32 Port = 63617;
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
                                                     },
                                                     SkipDesinationNameFormatting = true
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
                    // destinationQueue.SkipStompDesinationNameFormatting = true;
                    using ( var producer = session.CreateProducer( destinationQueue ) )
                    {
                        producer.DeliveryMode = MessageDeliveryMode.Persistent;

                        // Send a message to the destination
                        var message = session.CreateTextMessage( RandomValueEx.GetRandomString() );
                        message.StompTimeToLive = TimeSpan.FromMinutes( 1 );
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
                        {
                            Console.WriteLine( "Unexpected message type: " + msg.GetType()
                                                                                .Name );
                            if ( !( msg is IBytesMessage byteMessage ) )
                                throw new Exception( "Message is of unknown type." );

                            var s = Encoding.UTF8.GetString( byteMessage.Content );
                            Console.WriteLine( $"Message received: {s}" );

                            msg.Acknowledge();
                            foreach ( var key in msg.Headers.Keys )
                                Console.WriteLine( $"\t{msg.Headers[key]}" );
                        }
                    }
                }
            }
        }
    }
}