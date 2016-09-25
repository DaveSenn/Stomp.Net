

#region Usings

using System;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     A Factory of IConnection objects
    /// </summary>
    public interface IConnectionFactory
    {
        #region Properties

        /// <summary>
        ///     Get/or set the broker Uri.
        /// </summary>
        Uri BrokerUri { get; set; }

        /// <summary>
        ///     Get/or set the redelivery policy that new IConnection objects are
        ///     assigned upon creation.
        /// </summary>
        IRedeliveryPolicy RedeliveryPolicy { get; set; }

        /// <summary>
        ///     A Delegate that is called each time a Message is dispatched to allow the client to do
        ///     any necessary transformations on the received message before it is delivered.  The
        ///     ConnectionFactory sets the provided delegate instance on each Connection instance that
        ///     is created from this factory, each connection in turn passes the delegate along to each
        ///     Session it creates which then passes that along to the Consumers it creates.
        /// </summary>
        ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        /// <summary>
        ///     A delegate that is called each time a Message is sent from this Producer which allows
        ///     the application to perform any needed transformations on the Message before it is sent.
        ///     The ConnectionFactory sets the provided delegate instance on each Connection instance that
        ///     is created from this factory, each connection in turn passes the delegate along to each
        ///     Session it creates which then passes that along to the Producers it creates.
        /// </summary>
        ProducerTransformerDelegate ProducerTransformer { get; set; }

        #endregion

        /// <summary>
        ///     Creates a new connection
        /// </summary>
        IConnection CreateConnection();

        /// <summary>
        ///     Creates a new connection with the given user name and password
        /// </summary>
        IConnection CreateConnection( String userName, String password );
    }
}