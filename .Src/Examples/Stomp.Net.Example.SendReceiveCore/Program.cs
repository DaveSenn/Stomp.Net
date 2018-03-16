#region Usings

using System;
using System.Text;

#endregion

namespace Stomp.Net.Example.SendReceiveCore
{
    public class Program
    {
        #region Constants

        private const String Destination = "TestQ";
        private const String Host = "atmfutura2";
        private const String Password = "password";

        //private const Int32 Port = 61613;
        private const Int32 Port = 63617;

        private const String User = "admin";

        #endregion

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
                                                     SkipDesinationNameFormatting = false, // Determines whether the destination name formatting should be skipped or not.
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
                    // destinationQueue.SkipStompDesinationNameFormatting = true;
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
                        Console.WriteLine( $"\n\nMessage received: {s}" );

                        msg.Acknowledge();
                        foreach ( var key in msg.Headers.Keys )
                            Console.WriteLine( $"\t{msg.Headers[key]}" );
                    }
                }
            }
        }
    }
}