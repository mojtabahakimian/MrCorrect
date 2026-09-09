using Dapper;
using Microsoft.Data.SqlClient;
using Prg_SendInvoice.CNNMANAGER;
using System.Data;
using static Dapper.SqlMapper;

namespace Prg_Proccessy.CNNMANAGER
{
    public class CL_ConcurrencyManager : IDisposable
    {
        private static readonly string DbmsFullPathFile = "C:\\CORRECT\\DBMSLOG3.txt";
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;
        private bool _isExternalTransaction;
        private string SQLCNN = null;

        /// <summary>
        /// دستورهای نوشتنی تراکنش که هنوز commit نشده‌اند.
        ///
        /// اگر بلافاصله بعد از Execute ثبت شوند، rollback بعدی (که در
        /// <see cref="Dispose"/> هم به‌صورت خودکار رخ می‌دهد) ردیف‌هایی در
        /// سابقه جا می‌گذارد که هرگز در دیتابیس نوشته نشده‌اند.
        /// </summary>
        private readonly List<(string Sql, object Parameters)> _pendingAudit = new();

        /// <summary>
        /// Ignore Transaction way go like CNNMANAGER Open then Close immediately
        /// </summary>
        public bool OnceStartCloseQuery { get; set; } = false;
        // سازنده اصلی که از رشته اتصال استفاده می‌کند
        public CL_ConcurrencyManager(string CONNECTION_STR, bool _OnceStartCloseQuery_ = false)
        {
            _connection = new SqlConnection(CONNECTION_STR);

            OnceStartCloseQuery = _OnceStartCloseQuery_;

            SQLCNN = CONNECTION_STR;
        }

        // سازنده اضافه که یک اتصال موجود را می‌گیرد
        public CL_ConcurrencyManager(IDbConnection existingConnection)
        {
            if (existingConnection == null)
                throw new ArgumentNullException(nameof(existingConnection));

            _connection = existingConnection;
            if (!OnceStartCloseQuery)
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
          
            SQLCNN = _connection.ConnectionString;
        }

        /// <summary>
        /// برای اینکه از خطا Multiple DataRedaer جلوگیری کنیم باید این رو فعال کنیم
        /// </summary>
        /// <param name="_OnceStartCloseQuery_"></param>
        public void SetTransctionStating(bool _OnceStartCloseQuery_ = false)
        {
            OnceStartCloseQuery = _OnceStartCloseQuery_;
        }
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }
            if (!OnceStartCloseQuery)
            {
                _transaction = _connection.BeginTransaction(isolationLevel);
            }
       
            _isExternalTransaction = false;
        }
        public void BeginTransaction(IDbTransaction externalTransaction)
        {
            if (externalTransaction == null)
                throw new ArgumentNullException(nameof(externalTransaction));
            if (externalTransaction.Connection != _connection)
                throw new ArgumentException("The provided transaction does not belong to the current connection.", nameof(externalTransaction));
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = externalTransaction;
            _isExternalTransaction = true;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerable<T> SqlQuery<T>(string sql, object parameters = null, bool HighLockTable = false, string? _TableName_ = null)
        {
            if (OnceStartCloseQuery)
            {
                using (SqlConnection db = new SqlConnection(SQLCNN))
                {
                    try
                    {
                        db.Open();
                        var rows = db.Query<T>(sql, parameters, commandTimeout: 3600);
                        // «INSERT ... OUTPUT INSERTED.id» هم از مسیر Query می‌گذرد.
                        Prg_Proccessy.AUDIT.AuditSqlSniffer.Observe(sql, parameters);
                        return rows;
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, sql);
                        throw; // Re-throw the exception to handle it further up the call stack
                    }
                    finally
                    {
                        db?.Close(); db?.Dispose();
                    }
                }
            }
            else
            {
                EnsureTransaction();
                try
                {
                    if (HighLockTable && !string.IsNullOrEmpty(_TableName_))
                    {
                        _connection.Query($"SELECT 1 FROM {_TableName_} WITH (TABLOCKX, HOLDLOCK)", parameters, transaction: _transaction);
                    }

                    var rows = _connection.Query<T>(sql, parameters, transaction: _transaction, commandTimeout: 3600);
                    QueueAudit(sql, parameters);
                    return rows;
                }
                catch (Exception ex)
                {
                    LogError(ex, sql);
                    throw;
                }
            }

        }

        [System.Diagnostics.DebuggerStepThrough]
        public int ExecuteSqlCommand(string sql, object parameters = null, bool HighLockTable = false, string? _TableName_ = null)
        {
            if (OnceStartCloseQuery)
            {
                const int maxRetries = 15;
                for (int attempt = 0; ; attempt++)
                {
                    using (var db = new SqlConnection(SQLCNN))
                    {
                        try
                        {
                            db.Open();
                            var result = db.Execute(sql, parameters, commandTimeout: 3600);
                            Prg_Proccessy.AUDIT.AuditSqlSniffer.Observe(sql, parameters);
                            return result;
                        }
                        catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                        {
                            var baseMs = 100 * (1 << Math.Min(attempt, 5));
                            Thread.Sleep(baseMs + Random.Shared.Next(baseMs));
                            continue;
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, sql);
                            throw; // Re-throw the exception to handle it further up the call stack
                        }
                        finally
                        {
                            db?.Close(); db?.Dispose();
                        }
                    }
                }
            }
            else
            {
                EnsureTransaction();
                try
                {
                    if (HighLockTable && !string.IsNullOrEmpty(_TableName_))
                    {
                        _connection.Query($"SELECT 1 FROM {_TableName_} WITH (TABLOCKX, HOLDLOCK)", parameters, transaction: _transaction);
                    }

                    var affected = _connection.Execute(sql, parameters, transaction: _transaction, commandTimeout: 3600);
                    // فقط صف می‌شود؛ ثبت واقعی هنگام Commit انجام می‌گیرد.
                    QueueAudit(sql, parameters);
                    return affected;
                }
                catch (Exception ex)
                {
                    LogError(ex, sql);
                    throw;
                }
            }

        }

        public IEnumerable<T> ExecuteQueryNoTransaction<T>(string sql, object parameters = null)
        {
            try
            {
                return _connection.Query<T>(sql, parameters, commandTimeout: 3600);
            }
            catch (Exception ex)
            {
                LogError(ex, sql);
                throw;
            }
        }

        public void Commit()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }
            if (_isExternalTransaction)
            {
                throw new InvalidOperationException("Cannot commit an externally provided transaction. Please commit it externally.");
            }
            try
            {
                _transaction.Commit();
                FlushAudit(committed: true);
            }
            catch (Exception ex)
            {
                FlushAudit(committed: false);
                LogError(ex, "Commit Transaction Exception");
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>نگه‌داشتن دستور تا زمان Commit. سقف دارد تا تراکنش بزرگ حافظه نگیرد.</summary>
        private void QueueAudit(string sql, object parameters)
        {
            try
            {
                // SELECTها نباید وارد صف شوند: مسیر SqlQuery هم از اینجا
                // می‌گذرد و بدون این غربال، یک تراکنش پرخوانش سقف صف را پر
                // می‌کرد و نوشتن‌های واقعی بی‌صدا از سابقه می‌افتادند.
                if (!Prg_Proccessy.AUDIT.AuditSqlSniffer.LooksLikeWrite(sql)) return;

                // تراکنش خارجی: Commit و Rollback این کلاس برای تراکنش خارجی
                // عمداً استثنا پرتاب می‌کنند، پس صف هرگز با committed=true
                // تخلیه نمی‌شود و Dispose آن را دور می‌ریخت — یعنی تمام
                // نوشتن‌های این مسیر (AUTO_BAZ) از سابقه غایب می‌شدند.
                //
                // اینجا فوراً ثبت می‌شود. معامله‌اش این است که اگر تراکنشِ
                // بیرونی rollback شود، چند ردیف سابقه‌ی بی‌پشتوانه می‌ماند؛
                // برای یک سیستم سابقه، «کمِ اضافه» از «گمِ کامل» بهتر است.
                if (_isExternalTransaction)
                {
                    Prg_Proccessy.AUDIT.AuditSqlSniffer.Observe(sql, parameters);
                    return;
                }

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

        public void Rollback()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }
            if (_isExternalTransaction)
            {
                throw new InvalidOperationException("Cannot rollback an externally provided transaction. Please rollback it externally.");
            }
            try
            {
                _transaction.Rollback();
            }
            catch (Exception ex)
            {
                LogError(ex, "Rollback Transaction");
                throw;
            }
            finally
            {
                // عمداً چیزی ثبت نمی‌شود: این دستورها هرگز در دیتابیس ننشستند.
                FlushAudit(committed: false);
                _transaction.Dispose();
                _transaction = null;
            }
        }

        private void EnsureTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction. Please call BeginTransaction() first.");
            }
        }

        private void LogError(Exception er, string sql)
        {
            try
            {
                Console.WriteLine("Error in SQL execution: " + er.Message + " SQL: " + sql);
                string logMessage = $"\n {DateTime.Now}  \n Error in SQL execution :[ {sql} ]\n" +
                    $"{er.Message} \n {er.InnerException} \n {er.StackTrace} \n {er.Source} \n" +
                    $"\n Method Name: {(er.TargetSite != null ? er.TargetSite.Name : "N/A")} \n Base Exception: {er.GetBaseException().Message} \n Exception Data: {er.Data}" +
                    $"\n Help Link: {er.HelpLink} \n ExceptionType: {er.GetType().FullName} \n [[[ {SQLCNN} ]]]";
                File.AppendAllText(DbmsFullPathFile, logMessage);
            }
            catch
            {
                // Swallow logging errors.
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            // Dispose بدون Commit یعنی rollback؛ صف سابقه باید دور ریخته شود.
            FlushAudit(committed: false);

            try
            {
                if (!_isExternalTransaction)
                {
                    _transaction?.Rollback();
                }
            }
            catch { }
            finally
            {
                if (!_isExternalTransaction)
                {
                    _transaction?.Dispose();
                }
                _connection?.Close();
                _connection?.Dispose();
                _transaction = null;
                _disposed = true;
                SQLCNN = null;
            }
        }
    }
}
#region Example
/*
 Usage Examples
 Using an Internal Transaction (Default Behavior)
 csharp
 Copy
 using (var manager = new CL_ConcurrencyManager("your_connection_string"))
 {
     // Create an internal transaction.
     manager.BeginTransaction();

     // Execute queries/commands within the transaction.
     var result = manager.SqlQuery<string>("SELECT TOP 1 SomeColumn FROM SomeTable WITH (UPDLOCK, HOLDLOCK)");

     // ... other operations ...

     // Commit the transaction.
     manager.Commit();
 }


 Using an External Transaction
 csharp
 Copy
 // Create the connection and transaction externally.
 using (var connection = new SqlConnection("your_connection_string"))
 {
     connection.Open();
     using (var externalTransaction = connection.BeginTransaction())
     {
         using (var manager = new CL_ConcurrencyManager("your_connection_string"))
         {
             // Supply the external transaction via the overloaded BeginTransaction method.
             manager.BeginTransaction(externalTransaction);

             // Execute your queries/commands within the supplied transaction.
             var result = manager.SqlQuery<string>("SELECT TOP 1 SomeColumn FROM SomeTable WITH (UPDLOCK, HOLDLOCK)");

             // ... other operations ...
         }

         // Commit or rollback the external transaction outside the manager.
         externalTransaction.Commit(); // or externalTransaction.Rollback();
     }
 }
 */
#endregion
