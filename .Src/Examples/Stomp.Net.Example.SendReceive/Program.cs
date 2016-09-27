#region Usings

using System;
using Apache.NMS;
using Extend;

#endregion

namespace Stomp.Net.Example.Producer
{
    public class Program
    {
        #region Constants

        private const String Destination = "TestQ";
        private const String Host = "atmfutura";
        //private const String Host = "parsnip";
        private const String Password = "password";
        private const Int32 Port = 61902;
        //private const Int32 Port = 63615;
        private const String User = "admin";

        #endregion

        public static void Main( String[] args )
        {
            SendReceive();

            Console.WriteLine( "Press <enter> to exit." );
            Console.ReadLine();
        }

        private static void SendReceive()
        {
            var brokerUri = "tcp://" + Host + ":" + Port;
            var factory = new ConnectionFactory( brokerUri, new StompConnectionSettings { UserName = User, Password = Password } );

            // Create connection for both requests and responses
            var connection = factory.CreateConnection();
            connection.Start();

            // Create session for both requests and responses
            var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge );

            IDestination destinationQueue = session.GetQueue( Destination );
            var producer = session.CreateProducer( destinationQueue );
            producer.DeliveryMode = MessageDeliveryMode.NonPersistent;

            var message = session.CreateTextMessage( RandomValueEx.GetRandomString() );
            producer.Send( message );
            Console.WriteLine( "Message sent" );

            IDestination sourceQueue = session.GetQueue( Destination );
            var consumer = session.CreateConsumer( sourceQueue );

            var msg = consumer.Receive();
            if ( msg is ITextMessage )
            {
                msg.Acknowledge();
                Console.WriteLine( "Message received" );
            }
            else
                Console.WriteLine( "Unexpected message type: " + msg.GetType()
                                                                    .Name );

            connection.Close();
        }
    }
}