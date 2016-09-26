#if !NETCF

#region Usings

using System;
using System.Transactions;

#endregion

#endif

namespace Apache.NMS
{
    /// <summary>
    ///     The INetTxSession interface extends the capability of Session by adding access to a NMS
    ///     provider's support for the Distributed Transactions (optional).  The transaction support
    ///     leverages the .NET Frameworks System.Transactions API.
    ///     The NMS Provider implements this interface by participating in the current ambient transaction
    ///     as defined by the System.Transactions.Transaction.Current static member.  Whenever a new
    ///     Transaction is entered the NMS provider should enlist in that transaction.  When there is no
    ///     ambient transaction then the NMS Provider should allow the INetTxSession instance to behave
    ///     as a session that is in Auto Acknowledge mode.
    ///     Calling the Commit or Rollback methods on a INetTxSession instance should throw an exception
    ///     as those operations are controlled by the Transaction Manager.
    ///     The INetTxSession interface is optional. NMS providers are not required to support this
    ///     interface. This interface is for use by NMS providers to support transactional environments.
    /// </summary>
    public interface INetTxSession : ISession
    {
#if !NETCF
        /// <summary>
        ///     Enlist the Session in the specified Transaction.
        ///     If the Session is already enlisted in a Transaction or there is an Ambient
        ///     Transaction and the given TX is not that Transaction then an exception should
        ///     be thrown.
        /// </summary>
        void Enlist( Transaction tx );

        Boolean EnlistsMsDtcNativeResource { get; set; }
#endif
    }
}