#region Usings

using System;
using System.Collections;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp
{
    public class TransactionContext
    {
        #region Fields

        private readonly Session _session;
        private readonly ArrayList _synchronizations = ArrayList.Synchronized( new ArrayList() );

        #endregion

        #region Properties

        public Boolean InTransaction => TransactionId != null;

        public TransactionId TransactionId { get; private set; }

        #endregion

        #region Ctor

        public TransactionContext( Session session )
        {
            _session = session;
        }

        #endregion

        /// <summary>
        ///     Method AddSynchronization
        /// </summary>
        public void AddSynchronization( ISynchronization synchronization ) => _synchronizations.Add( synchronization );

        public void Begin()
        {
            if ( !InTransaction )
            {
                TransactionId = _session.Connection.CreateLocalTransactionId();

                var info = new TransactionInfo();
                info.ConnectionId = _session.Connection.ConnectionId;
                info.TransactionId = TransactionId;
                info.Type = (Int32) TransactionType.Begin;

                _session.Connection.Oneway( info );

                TransactionStartedListener?.Invoke( _session );
            }
        }

        public void Commit()
        {
            if ( !InTransaction )
                throw new NmsException( "Invalid State: Not Currently in a Transaction" );

            BeforeEnd();

            var info = new TransactionInfo();
            info.ConnectionId = _session.Connection.ConnectionId;
            info.TransactionId = TransactionId;
            info.Type = (Int32) TransactionType.Commit;

            TransactionId = null;
            _session.Connection.SyncRequest( info );

            AfterCommit();
            _synchronizations.Clear();
        }

        public void RemoveSynchronization( ISynchronization synchronization ) => _synchronizations.Remove( synchronization );

        public void ResetTransactionInProgress()
        {
            if ( InTransaction )
            {
                TransactionId = null;
                _synchronizations.Clear();
            }
        }

        public void Rollback()
        {
            if ( !InTransaction )
                throw new NmsException( "Invalid State: Not Currently in a Transaction" );

            BeforeEnd();

            var info = new TransactionInfo();
            info.ConnectionId = _session.Connection.ConnectionId;
            info.TransactionId = TransactionId;
            info.Type = (Int32) TransactionType.Rollback;

            TransactionId = null;
            _session.Connection.SyncRequest( info );

            AfterRollback();
            _synchronizations.Clear();
        }

        private void AfterCommit()
        {
            lock ( _synchronizations.SyncRoot )
            {
                foreach ( ISynchronization synchronization in _synchronizations )
                    synchronization.AfterCommit();

                TransactionCommittedListener?.Invoke( _session );
            }
        }

        private void AfterRollback()
        {
            lock ( _synchronizations.SyncRoot )
            {
                foreach ( ISynchronization synchronization in _synchronizations )
                    synchronization.AfterRollback();

                TransactionRolledBackListener?.Invoke( _session );
            }
        }

        private void BeforeEnd()
        {
            lock ( _synchronizations.SyncRoot )
                foreach ( ISynchronization synchronization in _synchronizations )
                    synchronization.BeforeEnd();
        }

        #region Transaction State Events

        public event SessionTxEventDelegate TransactionStartedListener;
        public event SessionTxEventDelegate TransactionCommittedListener;
        public event SessionTxEventDelegate TransactionRolledBackListener;

        #endregion
    }
}