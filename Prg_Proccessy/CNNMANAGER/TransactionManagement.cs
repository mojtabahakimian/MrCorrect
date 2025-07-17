using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Prg_Proccessy.CNNMANAGER
{
    /// <summary>
    /// var tm = new TransactionManagement(SqlCnn); ||| tm.StartTransaction(); ||| tm.DoCommit() / tm.DoRollback(); ||| tm.Dispose(); at the End
    /// </summary>
    public class TransactionManagement : IDisposable
    {
        private SqlConnection _connection;
        private IDbTransaction _transaction;
        public TransactionManagement(string connectionString)
        {
            _connection = new SqlConnection(connectionString);

            _connection?.Open();
            _transaction = _connection.BeginTransaction();
            ////_transaction = _connection.BeginTransaction(IsolationLevel.Serializable);
        }
        //public void StartTransaction() { }

        [System.Diagnostics.DebuggerStepThrough]
        public int ExecuteSqlCommandCtc(string sql, object parameters = null)
        {
            return _connection.Execute(sql, parameters, _transaction, commandTimeout: 3600);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerable<T> SqlQueryCtc<T>(string sql, object parameters = null)
        {
            return _connection.Query<T>(sql, parameters, _transaction, commandTimeout: 3600);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public async Task<int> ExecuteSqlCommandCtcAsync(string sql, object parameters = null)
        {
            return await _connection.ExecuteAsync(sql, parameters, _transaction, commandTimeout: 3600);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public async Task<IEnumerable<T>> SqlQueryCtcAsync<T>(string sql, object parameters = null)
        {
            return await _connection.QueryAsync<T>(sql, parameters, _transaction, commandTimeout: 3600);
        }

        public void DoCommit(bool _AutomaticDispose = true)
        {
            _transaction?.Commit();

            if (_AutomaticDispose)
            {
                Dispose();
            }
        }
        public void DoRollback(bool _AutomaticDispose = true)
        {
            _transaction?.Rollback();

            if (_AutomaticDispose)
            {
                Dispose();
            }
        }
        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Close();
        }
    }
}
