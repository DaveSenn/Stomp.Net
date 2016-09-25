

using System;

namespace Apache.NMS
{
    /// <summary>
    ///     Some application servers provide support for use in a .NET transactions (optional).
    ///     To include NMS API transactions in a MSDTC transaction, an application server requires a
    ///     .NET Transaction aware NMS provider that is capable of mapping the MSDTC transaction model
    ///     into operations that are supported by the application server. An NMS provider exposes its
    ///     .NET Transaction support using an INetTxConnectionFactory object, which an application
    ///     server uses to create INetTxConnection objects.
    ///     The INetTxConnectionFactory interface is optional. NMS providers are not required to support this
    ///     interface. This interface is for use by NMS providers to support transactional environments.
    /// </summary>
    public interface INetTxConnectionFactory : IConnectionFactory
    {
        /// <summary>
        ///     Creates a new connection
        /// </summary>
        INetTxConnection CreateNetTxConnection();

        /// <summary>
        ///     Creates a new connection with the given user name and password
        /// </summary>
        INetTxConnection CreateNetTxConnection( String userName, String password );
    }
}