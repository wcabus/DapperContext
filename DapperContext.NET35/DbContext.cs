using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Dapper
{
    /// <summary>
    /// A database context class for Dapper (https://github.com/SamSaffron/dapper-dot-net), based on http://blog.gauffin.org/2013/01/ado-net-the-right-way/#.UpWLPMSkrd2
    /// </summary>
    public class DbContext : IDisposable
    {
        private IDbConnection _connection;
        private readonly DbConnectionFactory _connectionFactory;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private readonly LinkedList<UnitOfWork> _workItems = new LinkedList<UnitOfWork>();

        /// <summary>
        /// <para>Default constructor.</para>
        /// <para>Uses the <paramref name="connectionStringName"/> to instantiate a <see cref="DbConnectionFactory"/>. This factory will be used to create connections to a database.</para>
        /// </summary>
        /// <param name="connectionStringName">The name of the connectionstring as defined in a app/web.config file's connectionstrings section.</param>
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

#if !CSHARP30
        /// <summary>
        /// Return a list of dynamic objects, reader is closed after the call
        /// </summary>
        public IEnumerable<dynamic> Query(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query<dynamic>(_connection, sql, param, GetCurrentTransaction(), true, commandTimeout, commandType);
        }
#else
        /// <summary>
        /// Return a list of dynamic objects, reader is closed after the call
        /// </summary>
        public IEnumerable<IDictionary<string, object>> Query(string sql, object param)
        {
            return Query(sql, param, null, null);
        }

        /// <summary>
        /// Return a list of dynamic objects, reader is closed after the call
        /// </summary>
        public IEnumerable<IDictionary<string, object>> Query(string sql, object param, CommandType? commandType)
        {
            return Query(sql, param, null, commandType);
        }

        /// <summary>
        /// Return a list of dynamic objects, reader is closed after the call
        /// </summary>
        public IEnumerable<IDictionary<string, object>> Query(string sql, object param, int? commandTimeout, CommandType? commandType)
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return _connection.Query(sql, param, GetCurrentTransaction(), true, commandTimeout, commandType);
        }
#endif

#if CSHARP30
        /// <summary>
        /// Execute parameterized SQL  
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public int Execute(string sql, object param)
        {
            return Execute(sql, param, null, null);
        }

        /// <summary>
        /// Execute parameterized SQL
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public int Execute(string sql, object param, CommandType commandType)
        {
            return Execute(sql, param, null, commandType);
        }
        
        /// <summary>
        /// Executes a query, returning the data typed as per T
        /// </summary>
        /// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public IEnumerable<T> Query<T>(string sql, object param)
        {
            return Query<T>(sql, param, null, null);
        }
        
        /// <summary>
        /// Executes a query, returning the data typed as per T
        /// </summary>
        /// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public IEnumerable<T> Query<T>(string sql, object param, CommandType commandType)
        {
            return Query<T>(sql, param, null, commandType);
        }
        
        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn
        /// </summary>
        public SqlMapper.GridReader QueryMultiple(string sql, object param)
        {
            return QueryMultiple(sql, param, null, null);
        }

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn
        /// </summary>
        public SqlMapper.GridReader QueryMultiple(string sql, object param, CommandType commandType)
        {
            return QueryMultiple(sql, param, null, commandType);
        }
#endif

#if CSHARP30
        public IEnumerable<T> Query<T>(string sql, object param, int? commandTimeout, CommandType? commandType)
#else
        public IEnumerable<T> Query<T>(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null)
#endif
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query<T>(_connection, sql, param, GetCurrentTransaction(), true, commandTimeout, commandType);
        }

#if CSHARP30
        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param, string splitOn, int? commandTimeout, CommandType? commandType)
#else
        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
#endif
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query(_connection, sql, map, param, GetCurrentTransaction(), true, splitOn, commandTimeout, commandType);
        }

#if CSHARP30
        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param, string splitOn, int? commandTimeout, CommandType? commandType)
#else
        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
#endif
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query(_connection, sql, map, param, GetCurrentTransaction(), true, splitOn, commandTimeout, commandType);
        }

#if CSHARP30
        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param, string splitOn, int? commandTimeout, CommandType? commandType)
#else
        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
#endif
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query(_connection, sql, map, param, GetCurrentTransaction(), true, splitOn, commandTimeout, commandType);
        }

#if !CSHARP30
        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query(_connection, sql, map, param, GetCurrentTransaction(), true, splitOn, commandTimeout, commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, dynamic param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query(_connection, sql, map, param, GetCurrentTransaction(), true, splitOn, commandTimeout, commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, dynamic param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.Query(_connection, sql, map, param, GetCurrentTransaction(), true, splitOn, commandTimeout, commandType);
        }
#endif

#if CSHARP30
        public SqlMapper.GridReader QueryMultiple(string sql, object param, int? commandTimeout, CommandType? commandType)
#else
        public SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null)
#endif
        {
            CreateOrReuseConnection();
            //Dapper will open and close the connection for us if necessary.
            return SqlMapper.QueryMultiple(_connection, sql, param, GetCurrentTransaction(), commandTimeout, commandType);
        }

#if CSHARP30
        public int Execute(string sql, object param, int? commandTimeout, CommandType? commandType)
#else
        public int Execute(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null)
#endif
        {
            CreateOrReuseConnection();
            //Dapper expects a connection to be open when calling Execute, so we'll have to open it.
            bool wasClosed = _connection.State == ConnectionState.Closed;
            if (wasClosed) _connection.Open();
            try
            {
                return SqlMapper.Execute(_connection, sql, param, GetCurrentTransaction(), commandTimeout, commandType);
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

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/>.
        /// </summary>
        public void Dispose()
        {
            //Use an upgradeable lock, because when we dispose a unit of work,
            //one of the removal methods will be called (which enters a write lock)
            _rwLock.EnterUpgradeableReadLock();
            try
            {
                while (_workItems.Any())
                {
                    var workItem = _workItems.First;
                    workItem.Value.Dispose(); //rollback, will remove the item from the LinkedList because it calls either RemoveTransaction or RemoveTransactionAndCloseConnection
                }
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
