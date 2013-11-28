using System;
using System.Data;

namespace Dapper
{
    public class UnitOfWork : IDisposable
    {
        private IDbTransaction _transaction;
        private readonly Action<UnitOfWork> _onCommit;
        private readonly Action<UnitOfWork> _onRollback;

        public UnitOfWork(IDbTransaction transaction, Action<UnitOfWork> onCommitOrRollback) : this(transaction, onCommitOrRollback, onCommitOrRollback)
        {
        }

        public UnitOfWork(IDbTransaction transaction, Action<UnitOfWork> onCommit, Action<UnitOfWork> onRollback)
        {
            _transaction = transaction;
            _onCommit = onCommit;
            _onRollback = onRollback;
        }

        public IDbTransaction Transaction
        {
            get { return _transaction; }
        }

        public void SaveChanges()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Cannot call SaveChanges more than once on the same unit of work.");

            try {
                _transaction.Commit();
                _onCommit(this);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (_transaction == null) return;

            try
            {
                _transaction.Rollback();
                _onRollback(this);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }
}