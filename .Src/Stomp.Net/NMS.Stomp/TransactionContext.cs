#region Usings

using System;
using System.Collections.Concurrent;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp;

public class TransactionContext
{
    #region Ctor

    public TransactionContext( Session session ) => _session = session;

    #endregion

    /// <summary>
    ///     Method AddSynchronization
    /// </summary>
    public void AddSynchronization( ISynchronization synchronization )
    {
        if ( !_synchronizations.TryAdd( synchronization, synchronization ) )
            Tracer.Warn( "Failed to add synchronization." );
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
        _synchronizations.Clear();
    }

    public void ResetTransactionInProgress()
    {
        if ( !InTransaction )
            return;

        TransactionId = null;
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
        _synchronizations.Clear();
    }

    private void AfterCommit()
    {
        foreach ( var synchronization in _synchronizations )
            synchronization.Value.AfterCommit();

        TransactionCommittedListener?.Invoke( _session );
    }

    private void AfterRollback()
    {
        foreach ( var synchronization in _synchronizations )
            synchronization.Value.AfterRollback();

        TransactionRolledBackListener?.Invoke( _session );
    }

    private void BeforeEnd()
    {
        foreach ( var synchronization in _synchronizations )
            synchronization.Value.BeforeEnd();
    }

    #region Fields

    private readonly Session _session;
    private readonly ConcurrentDictionary<ISynchronization, ISynchronization> _synchronizations = new();

    #endregion

    #region Properties

    public Boolean InTransaction => TransactionId != null;

    public TransactionId TransactionId { get; private set; }

    #endregion

    #region Transaction State Events

    public event Action<ISession> TransactionStartedListener;
    public event Action<ISession> TransactionCommittedListener;
    public event Action<ISession> TransactionRolledBackListener;

    #endregion
}