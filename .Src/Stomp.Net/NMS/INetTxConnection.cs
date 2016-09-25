

#if !NETCF

#region Usings

using System;
using System.Transactions;

#endregion

#endif

namespace Apache.NMS
{
    /// <summary>
    ///     The INetTxConnection extends the functionality of the IConnection interface by
    ///     adding the createNetTxSession method (optional).
    ///     The INetTxConnection interface is optional. NMS providers are not required to support this
    ///     interface. This interface is for use by NMS providers to support transactional environments.
    /// </summary>
    public interface INetTxConnection : IConnection
    {
        /// <summary>
        ///     Creates a INetTxSession object.
        /// </summary>
        INetTxSession CreateNetTxSession();

#if !NETCF
        /// <summary>
        ///     Creates a INetTxSession object and enlists in the specified Transaction.
        /// </summary>
        INetTxSession CreateNetTxSession( Transaction tx );

        INetTxSession CreateNetTxSession( Boolean enlistsNativeMsDtcResource );

        INetTxSession CreateNetTxSession( Transaction tx, Boolean enlistsNativeMsDtcResource );
#endif
    }
}