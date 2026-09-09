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

        /// <summary>
        /// دستورهای نوشتنی که هنوز commit نشده‌اند.
        ///
        /// ثبت سابقه نمی‌تواند بلافاصله بعد از Execute انجام شود: اگر تراکنش
        /// بعداً rollback شود، ردیف‌هایی در سابقه می‌ماند که هرگز در دیتابیس
        /// نوشته نشده‌اند و بررسی‌کننده را گمراه می‌کند. پس دستورها اینجا
        /// نگه داشته می‌شوند و فقط هنگام commit موفق ثبت می‌گردند.
        /// </summary>
        private readonly List<(string Sql, object Parameters)> _pendingAudit = new();

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
                    var affected = _connection.Execute(sql, parameters, _transaction, commandTimeout: 3600);
                    // فقط صف می‌شود؛ ثبت واقعی هنگام commit انجام می‌گیرد.
                    QueueAudit(sql, parameters);
                    return affected;
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
                    var rows = _connection.Query<T>(sql, parameters, _transaction, commandTimeout: 3600);
                    // این متد فقط خواندن نیست: «INSERT ... OUTPUT INSERTED.id»
                    // هم از همین‌جا اجرا می‌شود.
                    QueueAudit(sql, parameters);
                    return rows;
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
                    var affected = await _connection.ExecuteAsync(sql, parameters, _transaction, commandTimeout: 3600);
                    QueueAudit(sql, parameters);
                    return affected;
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
                    var rows = await _connection.QueryAsync<T>(sql, parameters, _transaction, commandTimeout: 3600);
                    QueueAudit(sql, parameters);
                    return rows;
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

        /// <summary>نگه‌داشتن دستور تا زمان commit. سقف دارد تا تراکنش‌های خیلی بزرگ حافظه نگیرند.</summary>
        private void QueueAudit(string sql, object parameters)
        {
            try
            {
                // SELECTها نباید وارد صف شوند: مسیر SqlQueryCtc هم از اینجا
                // می‌گذرد و بدون این غربال، یک تراکنش پرخوانش سقف صف را پر
                // می‌کرد و نوشتن‌های واقعی بی‌صدا از سابقه می‌افتادند.
                if (!Prg_Proccessy.AUDIT.AuditSqlSniffer.LooksLikeWrite(sql)) return;

                if (_pendingAudit.Count < 200)
                {
                    _pendingAudit.Add((sql, parameters));
                }
            }
            catch { }
        }

        /// <summary>ثبت سابقه‌ی دستورهای تراکنشِ commit‌شده و خالی کردن صف.</summary>
        private void FlushAudit(bool committed)
        {
            try
            {
                if (committed)
                {
                    foreach (var item in _pendingAudit)
                    {
                        Prg_Proccessy.AUDIT.AuditSqlSniffer.Observe(item.Sql, item.Parameters);
                    }
                }
            }
            catch { }
            finally
            {
                _pendingAudit.Clear();
            }
        }

        public void DoCommit(bool _AutomaticDispose = true)
        {
            var committed = false;
            if (_transaction != null && _transaction.Connection != null)
            {
                _transaction?.Commit();
                committed = true;
            }

            FlushAudit(committed);

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

            // عمداً چیزی ثبت نمی‌شود: این دستورها هرگز در دیتابیس ننشستند.
            FlushAudit(committed: false);

            if (_AutomaticDispose)
            {
                Dispose();
            }
        }
        public void Dispose()
        {
            // Dispose بدون commit یعنی rollback؛ صف باید دور ریخته شود نه ثبت.
            FlushAudit(committed: false);

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
