#region Usings

using System;
using System.Collections.Generic;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp
{
    public class TransactionContext
    {
        #region Fields

        private readonly Session _session;
        private readonly List<Object> _synchronizations = new List<Object>();

        private readonly Object _synchronizationsLock = new Object();

        #endregion

        #region Properties

        public Boolean InTransaction => TransactionId != null;

        public TransactionId TransactionId { get; private set; }

        #endregion

        #region Ctor

        public TransactionContext( Session session ) => _session = session;

        #endregion

        /// <summary>
        ///     Method AddSynchronization
        /// </summary>
        public void AddSynchronization( ISynchronization synchronization )
        {
            lock ( _synchronizationsLock )
                _synchronizations.Add( synchronization );
        }

        public void Begin()
        {
            if ( InTransaction )
                return;
            TransactionId = _session.Connection.CreateLocalTransactionId();

            var info = new TransactionInfo
            {
                ConnectionId = _session.Connection.ConnectionId,
                TransactionId = TransactionId,
                Type = (Int32) TransactionType.Begin
            };

            _session.Connection.Oneway( info );

            TransactionStartedListener?.Invoke( _session );
        }

        public void Commit()
        {
            if ( !InTransaction )
                throw new StompException( "Invalid State: Not Currently in a Transaction" );

            BeforeEnd();

            var info = new TransactionInfo
            {
                ConnectionId = _session.Connection.ConnectionId,
                TransactionId = TransactionId,
                Type = (Int32) TransactionType.Commit
            };

            TransactionId = null;
            _session.Connection.SyncRequest( info );

            AfterCommit();
            lock ( _synchronizationsLock )
                _synchronizations.Clear();
        }

        public void ResetTransactionInProgress()
        {
            if ( !InTransaction )
                return;
            TransactionId = null;
            lock ( _synchronizationsLock )
                _synchronizations.Clear();
        }

        public void Rollback()
        {
            if ( !InTransaction )
                throw new StompException( "Invalid State: Not Currently in a Transaction" );

            BeforeEnd();

            var info = new TransactionInfo
            {
                ConnectionId = _session.Connection.ConnectionId,
                TransactionId = TransactionId,
                Type = (Int32) TransactionType.Rollback
            };

            TransactionId = null;
            _session.Connection.SyncRequest( info );

            AfterRollback();
            lock ( _synchronizationsLock )
                _synchronizations.Clear();
        }

        private void AfterCommit()
        {
            lock ( _synchronizationsLock )
            {
                foreach ( ISynchronization synchronization in _synchronizations )
                    synchronization.AfterCommit();

                TransactionCommittedListener?.Invoke( _session );
            }
        }

        private void AfterRollback()
        {
            lock ( _synchronizationsLock )
            {
                foreach ( ISynchronization synchronization in _synchronizations )
                    synchronization.AfterRollback();

                TransactionRolledBackListener?.Invoke( _session );
            }
        }

        private void BeforeEnd()
        {
            lock ( _synchronizationsLock )
                foreach ( ISynchronization synchronization in _synchronizations )
                    synchronization.BeforeEnd();
        }

        #region Transaction State Events

        public event Action<ISession> TransactionStartedListener;
        public event Action<ISession> TransactionCommittedListener;
        public event Action<ISession> TransactionRolledBackListener;

        #endregion
    }
}