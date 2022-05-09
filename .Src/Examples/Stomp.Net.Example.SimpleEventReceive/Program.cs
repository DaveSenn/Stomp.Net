#region Usings

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extend;

#endregion

namespace Stomp.Net.Example.SelectorsCore;

public class Program
{
    public static void Main( String[] args )
    {
        // Configure a logger to capture the output of the library
        Tracer.Trace = new ConsoleLogger();

        try
        {
            using var subscriber = new Subscriber();
            SendMessages();

            Console.WriteLine( $" [{Thread.CurrentThread.ManagedThreadId}] Start receiving messages." );

            subscriber.Start();

            Console.WriteLine( ResetEvent.Wait( 1.ToMinutes() ) ? "All messages received" : "Timeout :(" );
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

        return new(brokerUri,
                   new()
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
                   });
    }

    private static void SendMessages()
    {
        var factory = GetConnectionFactory();

        // Create connection for both requests and responses
        using var connection = factory.CreateConnection();
        // Open the connection
        connection.Start();

        // Create session for both requests and responses
        using var session = connection.CreateSession( AcknowledgementMode.IndividualAcknowledge );
        // Create a message producer
        IDestination destinationQueue = session.GetQueue( QueueName );
        using var producer = session.CreateProducer( destinationQueue );
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

    #region Nested Types

    private class Subscriber : IDisposable
    {
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
                    _noOfReceivedMessages++;

                    if ( _noOfReceivedMessages >= NoOfMessages )
                        ResetEvent.Set();
                }
            };
        }

        #region Fields

        private readonly Object _sync = new();
        private IConnection _connection;
        private IMessageConsumer _consumer;

        private Int32 _noOfReceivedMessages;
        private ISession _session;

        #endregion
    }

    #endregion

    #region Constants

    private const String Host = "host";

    private const Int32 NoOfMessages = 30;

    private const String Password = "password";

    private const Int32 Port = 63617;
    //private const Int32 Port = 61613;

    private const String QueueName = "TestQ";
    private const String User = "admin";

    private static readonly ManualResetEventSlim ResetEvent = new();

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