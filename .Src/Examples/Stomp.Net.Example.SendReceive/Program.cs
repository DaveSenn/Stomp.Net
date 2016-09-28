#region Usings

using System;
using System.Text;
using System.Threading;
using Apache.NMS;
using Extend;

#endregion

namespace Stomp.Net.Example.Producer
{
    public class Program
    {
        #region Constants

        private const String Destination = "PerfQ1";
        private const String Host = "atmfutura";
        //private const String Host = "parsnip";
        private const String Password = "password";
        private const Int32 Port = 61902;
        //private const Int32 Port = 63615;
        private const String User = "admin";

        #endregion

        public static void Main( String[] args )
        {
            //SendReceiveByte();
            //WaitForMessageTest();

            Console.WriteLine( "\n\n\n" );
            SendReceiveText();

            Console.WriteLine( "\n\nPress <enter> to exit." );
            Console.ReadLine();
        }

        private static void SendReceiveByte()
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
            producer.DeliveryMode = MessageDeliveryMode.Persistent;

            var message = session.CreateBytesMessage( Encoding.UTF8.GetBytes( "Hi from byte message => UTF8" ) );
            message.Headers["test"] = "test";
            producer.Send( message );
            Console.WriteLine( "Message sent\n" );

            IDestination sourceQueue = session.GetQueue( Destination );
            var consumer = session.CreateConsumer( sourceQueue );

            var msg = consumer.Receive();
            if ( msg is IBytesMessage )
            {
                Console.WriteLine( "Message received:\n" );
                Console.WriteLine( Encoding.UTF8.GetString( ( msg as IBytesMessage ).Content ) );

                msg.Acknowledge();
                foreach ( var key in msg.Headers.Keys )
                    Console.WriteLine( $"\t{msg.Headers[key]}" );
            }
            else
                Console.WriteLine( "Unexpected message type: " + msg.GetType()
                                                                    .Name );

            connection.Close();
        }

        private static void SendReceiveText()
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
            producer.DeliveryMode = MessageDeliveryMode.Persistent;

            var message = session.CreateTextMessage( RandomValueEx.GetRandomString() );
            message.Headers["test"] = "test";
            producer.Send( message );
            Console.WriteLine( "Message sent\n" );

            IDestination sourceQueue = session.GetQueue( Destination );
            var consumer = session.CreateConsumer( sourceQueue );

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

            connection.Close();
        }

        private static void WaitForMessageTest()
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
            producer.DeliveryMode = MessageDeliveryMode.Persistent;

            IDestination sourceQueue = session.GetQueue( Destination );
            var consumer = session.CreateConsumer( sourceQueue );

            const Int32 messageCount = 10;
            // Receive messages
            for ( var i = 0; i < messageCount; i++ )
            {
                var thread = new Thread( () =>
                                         {
                                             var msg = consumer.Receive();
                                             Console.WriteLine( $"Thread {Thread.CurrentThread.Name} received a message" );
                                             msg.Acknowledge();
                                         } ) { Name = $"Thread {i}" };
                thread.Start();
            }

            // Send messages
            for ( var i = 0; i < messageCount; i++ )
                new Thread( () =>
                            {
                                var message = session.CreateTextMessage( RandomValueEx.GetRandomString() );
                                message.Headers["test"] = "test";
                                producer.Send( message );
                            } ).Start();

            Thread.Sleep( 10.ToSeconds() );
            connection.Close();
        }
    }
}