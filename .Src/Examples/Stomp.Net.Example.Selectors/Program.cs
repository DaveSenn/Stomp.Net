#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Apache.NMS;
using Apache.NMS.Stomp;
using Extend;

#endregion

namespace Stomp.Net.Example.Selectors
{
    public class Program
    {
        #region Constants

        private const String Host = "atmfutura";
        private const String Password = "password";
        private const Int32 Port = 61902;
        private const String QueueName = "TestQ";
        private const String SelectorKey = "selectorProp";
        private const String User = "admin";
        private static readonly List<String> Selectors = new List<String> { "s1", "s2", "s3" };

        #endregion

        public static void Main( String[] args )
        {
            Send( "test", 1 );
            /*
             * Receive("test").Dispose();
            var sw = Stopwatch.StartNew();
            for ( var i = 0; i < 100; i++ )
                Send("1234", 1000);
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} => {sw.ElapsedMilliseconds}ms");
            Console.ReadLine();
            */

            Selectors.ForEach( x => Send( x, 5 ) );
            var consumers = Selectors.Select( Receive )
                                     .ToList();

            Thread.Sleep( 10000 );
            consumers.ForEach( x => x.Dispose() );

            Console.WriteLine( "Press <enter> to exit." );
            Console.ReadLine();
        }

        private static Consumer Receive( String selectorValue )
            => new Consumer( SelectorKey, selectorValue );

        private static void Send( String selectorValue, Int32 messageCount )
        {
            //var brokerUri = "stomp:tcp://" + Host + ":" + Port;
            var brokerUri = "tcp://" + Host + ":" + Port;
            brokerUri += "?transport.useInactivityMonitor=true&amp;trace=true";
            var factory = new ConnectionFactory( brokerUri );

            // Create connection for both requests and responses
            var connection = factory.CreateConnection( User, Password );
            connection.Start();

            // Create session for both requests and responses
            var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge );

            IDestination destinationQueue = session.GetQueue( QueueName );
            var producer = session.CreateProducer( destinationQueue );
            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

            for ( var i = 0; i < messageCount; i++ )
            {
                var message = session.CreateTextMessage( $"{i,0:000} => {RandomValueEx.GetRandomString()}" );
                message.Properties[SelectorKey] = selectorValue;
                producer.Send( message );
                //Console.WriteLine( "Message sent" );
            }

            connection.Close();
        }

        #region Nested Types

        private class Consumer : IDisposable
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

            private void Create( String selectorKey, String selector )
            {
                new Thread( () =>
                            {
                                var brokerUri = "tcp://" + Host + ":" + Port;
                                brokerUri += "?transport.useInactivityMonitor=true&amp;trace=true";
                                var factory = new ConnectionFactory( brokerUri );

                                // Create connection for both requests and responses
                                using ( var connection = factory.CreateConnection( User, Password ) )
                                {
                                    connection.Start();

                                    // Create session for both requests and responses
                                    using ( var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge ) )
                                    {
                                        String selectorString = $"{selectorKey} = '{selector}'";
                                        Console.WriteLine( $"Create consumer with selector {selectorString}" );
                                        IDestination responseQ = session.GetQueue( QueueName );
                                        var consumer = session.CreateConsumer( responseQ, selectorString );

                                        consumer.Listener += x => { Console.WriteLine( $"{selector}\tReceived message => {x.Properties[selectorKey]}" ); };

                                        while ( _running )
                                            Thread.Sleep( 1000 );
                                    }
                                }
                            } ).Start();
            }

            #region Implementation of IDisposable

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }

            protected virtual void Dispose( Boolean disposing )
            {
                if ( !disposing )
                    return;

                _running = false;
            }

            ~Consumer()
            {
                // Finalizer calls Dispose(false)
                Dispose( false );
            }

            #endregion
        }

        #endregion
    }
}