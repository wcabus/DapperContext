using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Dapper
{
    /// <summary>
    /// A database context class for Dapper (https://github.com/SamSaffron/dapper-dot-net), based on http://blog.gauffin.org/2013/01/ado-net-the-right-way/#.UpWLPMSkrd2
    /// </summary>
    public class DbContext
    {
        private IDbConnection _connection;
        private readonly DbConnectionFactory _connectionFactory;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private readonly LinkedList<UnitOfWork> _workItems = new LinkedList<UnitOfWork>();

        public DbContext(string connectionStringName)
        {
            _connectionFactory = new DbConnectionFactory(connectionStringName);
        }
        
        /// <summary>
        /// Ensures that a connection is ready for querying or creating transactions
        /// </summary>
        /// <remarks></remarks>
        private void CreateOrReuseConnection()
        {
            if (_connection != null) return;

            _connection = _connectionFactory.Create();
        }

        /// <summary>
        /// Creates a new <see cref="UnitOfWork"/>.
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> used for the transaction inside this unit of work. Default value: <see cref="IsolationLevel.ReadCommitted"/></param>
        /// <returns></returns>
        public UnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            CreateOrReuseConnection();

            //To create a transaction, our connection needs to be open.
            //If we need to open the connection ourselves, we're also in charge of closing it when this transaction commits or rolls back.
            //This will be done by RemoveTransactionAndCloseConnection in that case.
            bool wasClosed = _connection.State == ConnectionState.Closed;
            if (wasClosed) _connection.Open();
            
            try
            {
                UnitOfWork unit;
                IDbTransaction transaction = _connection.BeginTransaction(isolationLevel);
                
                if (wasClosed)
                    unit = new UnitOfWork(transaction, RemoveTransactionAndCloseConnection, RemoveTransactionAndCloseConnection);
                else
                    unit = new UnitOfWork(transaction, RemoveTransaction, RemoveTransaction);

                _rwLock.EnterWriteLock();
                _workItems.AddLast(unit);
                _rwLock.ExitWriteLock();

                return unit;
            }
            catch
            {
                //Close the connection if we're managing it, and if an exception is thrown when creating the transaction.
                if (wasClosed) _connection.Close();

                throw; //Rethrow the original transaction
            }
        }

        private IDbTransaction GetCurrentTransaction()
        {
            IDbTransaction currentTransaction = null;
            _rwLock.EnterReadLock();
            if (_workItems.Any()) currentTransaction = _workItems.First.Value.Transaction;
            _rwLock.ExitReadLock();

            return currentTransaction;
        }

        public IEnumerable<T> Query<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return _connection.Query<T>(sql, param, GetCurrentTransaction(), true, commandTimeout, commandType);
        }

        //TODO Add Query<TFirst, TSecond, TReturn> and others
            
        public SqlMapper.GridReader QueryMultiple(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return _connection.QueryMultiple(sql, param, GetCurrentTransaction(), commandTimeout, commandType);
        }

        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            CreateOrReuseConnection();
            //Dapper expects a connection to be open when calling Execute, so we'll have to open it.
            bool wasClosed = _connection.State == ConnectionState.Closed;
            if (wasClosed) _connection.Open();
            try
            {
                return _connection.Execute(sql, param, GetCurrentTransaction(), commandTimeout, commandType);
            }
            finally
            {
                if (wasClosed) _connection.Close();
            }
        }
        
        private void RemoveTransaction(UnitOfWork workItem)
        {
            _rwLock.EnterWriteLock();
            _workItems.Remove(workItem);
            _rwLock.ExitWriteLock();
        }

        private void RemoveTransactionAndCloseConnection(UnitOfWork workItem)
        {
            _rwLock.EnterWriteLock();
            _workItems.Remove(workItem);
            _rwLock.ExitWriteLock();

            _connection.Close();
        }
    }
}
