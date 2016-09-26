

#region Usings

using System;
using System.Collections;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp
{
    public enum TransactionType
    {
        Begin = 0,
        Commit = 1,
        Rollback = 2
    }
}

namespace Apache.NMS.Stomp
{
    public class TransactionContext
    {
        #region Fields

        private readonly Session session;
        private readonly ArrayList synchronizations = ArrayList.Synchronized( new ArrayList() );

        #endregion

        #region Properties

        public Boolean InTransaction
        {
            get { return TransactionId != null; }
        }

        public TransactionId TransactionId { get; private set; }

        #endregion

        #region Ctor

        public TransactionContext( Session session )
        {
            this.session = session;
        }

        #endregion

        /// <summary>
        ///     Method AddSynchronization
        /// </summary>
        public void AddSynchronization( ISynchronization synchronization ) => synchronizations.Add( synchronization );

        public void Begin()
        {
            if ( !InTransaction )
            {
                TransactionId = session.Connection.CreateLocalTransactionId();

                var info = new TransactionInfo();
                info.ConnectionId = session.Connection.ConnectionId;
                info.TransactionId = TransactionId;
                info.Type = (Int32) TransactionType.Begin;

                session.Connection.Oneway( info );

                if ( TransactionStartedListener != null )
                    TransactionStartedListener( session );
            }
        }

        public void Commit()
        {
            if ( !InTransaction )
                throw new NMSException( "Invalid State: Not Currently in a Transaction" );

            BeforeEnd();

            var info = new TransactionInfo();
            info.ConnectionId = session.Connection.ConnectionId;
            info.TransactionId = TransactionId;
            info.Type = (Int32) TransactionType.Commit;

            TransactionId = null;
            session.Connection.SyncRequest( info );

            AfterCommit();
            synchronizations.Clear();
        }

        public void RemoveSynchronization( ISynchronization synchronization ) => synchronizations.Remove( synchronization );

        public void ResetTransactionInProgress()
        {
            if ( InTransaction )
            {
                TransactionId = null;
                synchronizations.Clear();
            }
        }

        public void Rollback()
        {
            if ( !InTransaction )
                throw new NMSException( "Invalid State: Not Currently in a Transaction" );

            BeforeEnd();

            var info = new TransactionInfo();
            info.ConnectionId = session.Connection.ConnectionId;
            info.TransactionId = TransactionId;
            info.Type = (Int32) TransactionType.Rollback;

            TransactionId = null;
            session.Connection.SyncRequest( info );

            AfterRollback();
            synchronizations.Clear();
        }

        internal void AfterCommit()
        {
            lock ( synchronizations.SyncRoot )
            {
                foreach ( ISynchronization synchronization in synchronizations )
                    synchronization.AfterCommit();

                if ( TransactionCommittedListener != null )
                    TransactionCommittedListener( session );
            }
        }

        internal void AfterRollback()
        {
            lock ( synchronizations.SyncRoot )
            {
                foreach ( ISynchronization synchronization in synchronizations )
                    synchronization.AfterRollback();

                if ( TransactionRolledBackListener != null )
                    TransactionRolledBackListener( session );
            }
        }

        internal void BeforeEnd()
        {
            lock ( synchronizations.SyncRoot )
                foreach ( ISynchronization synchronization in synchronizations )
                    synchronization.BeforeEnd();
        }

        #region Transaction State Events

        public event SessionTxEventDelegate TransactionStartedListener;
        public event SessionTxEventDelegate TransactionCommittedListener;
        public event SessionTxEventDelegate TransactionRolledBackListener;

        #endregion
    }
}