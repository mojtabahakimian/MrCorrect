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
            const int maxRetries = 3;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return _connection.Execute(sql, parameters, _transaction, commandTimeout: 3600);
                }
                catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                {
                    // Deadlock detected - log and retry with exponential backoff
                    LogDeadlock(sql, attempt + 1, maxRetries, ex);
                    System.Threading.Thread.Sleep(200 * (attempt + 1));
                    continue;
                }
                catch (SqlException ex) when (ex.Number == 1205)
                {
                    // Max retries exceeded - log and rethrow
                    LogDeadlock(sql, maxRetries + 1, maxRetries, ex);
                    throw;
                }
            }

            return 0; // Should never reach here
        }
        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerable<T> SqlQueryCtc<T>(string sql, object parameters = null)
        {
            const int maxRetries = 3;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return _connection.Query<T>(sql, parameters, _transaction, commandTimeout: 3600);
                }
                catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                {
                    // Deadlock detected - log and retry with exponential backoff
                    LogDeadlock(sql, attempt + 1, maxRetries, ex);
                    System.Threading.Thread.Sleep(200 * (attempt + 1));
                    continue;
                }
                catch (SqlException ex) when (ex.Number == 1205)
                {
                    // Max retries exceeded - log and rethrow
                    LogDeadlock(sql, maxRetries + 1, maxRetries, ex);
                    throw;
                }
            }

            return Enumerable.Empty<T>(); // Should never reach here
        }

        [System.Diagnostics.DebuggerStepThrough]
        public async Task<int> ExecuteSqlCommandCtcAsync(string sql, object parameters = null)
        {
            const int maxRetries = 3;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await _connection.ExecuteAsync(sql, parameters, _transaction, commandTimeout: 3600);
                }
                catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                {
                    // Deadlock detected - log and retry with exponential backoff
                    LogDeadlock(sql, attempt + 1, maxRetries, ex);
                    await Task.Delay(200 * (attempt + 1));
                    continue;
                }
                catch (SqlException ex) when (ex.Number == 1205)
                {
                    // Max retries exceeded - log and rethrow
                    LogDeadlock(sql, maxRetries + 1, maxRetries, ex);
                    throw;
                }
            }

            return 0; // Should never reach here
        }

        [System.Diagnostics.DebuggerStepThrough]
        public async Task<IEnumerable<T>> SqlQueryCtcAsync<T>(string sql, object parameters = null)
        {
            const int maxRetries = 3;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await _connection.QueryAsync<T>(sql, parameters, _transaction, commandTimeout: 3600);
                }
                catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                {
                    // Deadlock detected - log and retry with exponential backoff
                    LogDeadlock(sql, attempt + 1, maxRetries, ex);
                    await Task.Delay(200 * (attempt + 1));
                    continue;
                }
                catch (SqlException ex) when (ex.Number == 1205)
                {
                    // Max retries exceeded - log and rethrow
                    LogDeadlock(sql, maxRetries + 1, maxRetries, ex);
                    throw;
                }
            }

            return Enumerable.Empty<T>(); // Should never reach here
        }

        public void DoCommit(bool _AutomaticDispose = true)
        {
            if (_transaction != null && _transaction.Connection != null)
            {
                _transaction?.Commit();
            }

            if (_AutomaticDispose)
            {
                Dispose();
            }
        }
        public void DoRollback(bool _AutomaticDispose = true)
        {
            if (_transaction != null && _transaction.Connection != null)
            {
                _transaction?.Rollback();
            }

            if (_AutomaticDispose)
            {
                Dispose();
            }
        }
        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;

            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }

        private static void LogDeadlock(string sql, int attempt, int maxRetries, SqlException ex)
        {
            try
            {
                string logPath = Path.Combine(Path.GetTempPath(), "DBMSLOG2.txt");
                string logMessage = $"\n\n------------------------------------------------------------" +
                                    $"\nDEADLOCK DETECTED - Attempt {attempt}/{maxRetries}" +
                                    $"\nTimestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}" +
                                    $"\nSQL Query: {sql}" +
                                    $"\nError Number: {ex.Number}" +
                                    $"\nError Message: {ex.Message}" +
                                    $"\nStack Trace: {ex.StackTrace}" +
                                    $"\n------------------------------------------------------------\n";

                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Ignore logging errors to prevent masking the original exception
            }
        }
    }
}
