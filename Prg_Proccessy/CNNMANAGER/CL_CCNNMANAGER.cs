using Dapper;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using static Dapper.SqlMapper;

namespace Prg_SendInvoice.CNNMANAGER
{
    public partial class CL_CCNNMANAGER
    {
        //: IDisposable
        //public void Dispose()
        //{
        //    GC.SuppressFinalize(this);
        //}

        private static readonly object _lock = new object();
        private static string _connectionStr;
        public static string CONNECTION_STR // = @"Data Source=MERCEDES\SQL2008;Initial Catalog=YAZD2024;Integrated Security=True;TrustServerCertificate=True;";
        {
            get
            {
                lock (_lock)
                {
                    return _connectionStr;
                }
            }
            set
            {
                lock (_lock)
                {
                    //_connectionStr = NormalizeConnectionString(value);
                    _connectionStr = value;
                }
            }
        }
        private static string NormalizeConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            var normalized = connectionString.Trim();

            if (!normalized.EndsWith(';'))
            {
                normalized += ";";
            }

            return normalized;
        }

        public static bool ConnectedToSQLDB { get; set; } = false;
        public static string ExtractConnectionString(string _fullConnectionString)
        {
            string fullConnectionString = _fullConnectionString;

            // Ensure it's a single line, trim the UDL file special characters/formatting, then split.
            var keyValuePairStrings = fullConnectionString
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(" - ", "")
                .Split(';');

            var connectionStringBuilder = new SqlConnectionStringBuilder();

            // Track whether certain keys have been provided.
            bool userIdProvided = false;
            bool passwordProvided = false;
            bool integratedSecurityProvided = false;

            foreach (string keyValuePair in keyValuePairStrings)
            {
                var pair = keyValuePair.Split('=');
                if (pair.Length == 2)
                {
                    string key = pair[0].Trim();
                    string value = pair[1].Trim();

                    switch (key)
                    {
                        case "Data Source":
                            connectionStringBuilder.DataSource = value;
                            break;
                        case "Initial Catalog":
                            connectionStringBuilder.InitialCatalog = value;
                            break;
                        case "User ID":
                            userIdProvided = true;
                            connectionStringBuilder.UserID = value;
                            break;
                        case "Password":
                            passwordProvided = true;
                            connectionStringBuilder.Password = value;
                            break;
                        case "Integrated Security":
                            integratedSecurityProvided = true;
                            //connectionStringBuilder.IntegratedSecurity = value.Equals("SSPI", StringComparison.OrdinalIgnoreCase);
                            //value.Equals("True", StringComparison.OrdinalIgnoreCase);
                            connectionStringBuilder.IntegratedSecurity = value.Equals("SSPI", StringComparison.OrdinalIgnoreCase) || value.Equals("True", StringComparison.OrdinalIgnoreCase);
                            break;
                    }
                }
            }

            // Append "TrustServerCertificate=True;" for .NET Core compatibility.
            connectionStringBuilder.TrustServerCertificate = true;

            connectionStringBuilder.MaxPoolSize = 1000; // ADDED: Prevent Connection Pool Exhaustion safety net

            // If the password was not provided, and we're not using Integrated Security, set it as an empty string.
            if (!passwordProvided && userIdProvided && !integratedSecurityProvided)
            {
                connectionStringBuilder.Password = string.Empty;
            }

            return connectionStringBuilder.ConnectionString + ";";

        }
        private static readonly int[] ConnectionErrorNumbers =
        {
            2,      // The system cannot find the file specified
            53,     // Network path not found
            64,     // Named pipe connection broken
            -2,     // Timeout expired
            26,     // Error locating server/instance specified
            121,    // Semaphore timeout period expired
            232,    // The pipe is being closed
            233,    // No process on the other end of the pipe
            4060,   // Cannot open database requested by the login
            17142,  // The server is paused
            18456   // Login failed for user
        };
        private static readonly int[] NonRetriableAuthenticationErrorNumbers =
        {
            18452,  // The login is from an untrusted domain and cannot be used with Integrated authentication
            18456,  // Login failed for user
            18470,  // Login failed for user because account is disabled
            18487,  // The password of the account has expired
            18488   // The password must be changed before logging on for the first time
        };

        private static bool IsNonRetriableAuthenticationError(SqlException? exception)
        {
            if (exception is null)
            {
                return false;
            }

            foreach (SqlError error in exception.Errors)
            {
                if (NonRetriableAuthenticationErrorNumbers.Contains(error.Number))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsConnectionRelated(SqlException? exception)
        {
            if (exception is null)
            {
                return false;
            }

            foreach (SqlError error in exception.Errors)
            {
                if (ConnectionErrorNumbers.Contains(error.Number))
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(exception.Message) &&
                exception.Message.IndexOf("A network-related", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (exception.InnerException is Win32Exception win32 &&
                   (win32.NativeErrorCode == 53 || win32.NativeErrorCode == 64 || win32.NativeErrorCode == 10060))
            {
                return true;
            }

            return false;
        }

        public CL_CCNNMANAGER()
        {
            //string path0 = @"C:\correct\CNR.udl";
            //string pathuspath = File.ReadLines(path0).Last();
            //var DataSource = "Data Source =" + CL_FUNCTIONS.GetBetweenStr(pathuspath, "Data Source =", ";") + ";";
            //var InitialCatalog = "Initial Catalog =" + CL_FUNCTIONS.GetBetweenStr(pathuspath, "Initial Catalog =", ";") + ";";
            //var IntegratedSecurity = "Integrated Security =" + CL_FUNCTIONS.GetBetweenStr(pathuspath, "Integrated Security =", ";") + ";";
            //var FinalCNN = DataSource + InitialCatalog + IntegratedSecurity;

            //CONNECTION_STR = FinalCNN + "TrustServerCertificate=True;";
        }
        const string DbmsFullPathFile = @"C:\CORRECT\DBMSLOG2.txt";

        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerable<TEntity> DoGetDataSQL<TEntity>(string sql, object parameters = null)
        {
            //using (SqlConnection db = new SqlConnection(CONNECTION_STR))
            //{
            //    try
            //    {
            //        db.Open();
            //        var results = db.Query<TEntity>(sql, parameters, commandTimeout: 3600);
            //        return results;
            //    }
            //    catch (Exception er)
            //    {
            //        LogSqlQuery(sql, er);

            //        throw; // Re-throw the exception to handle it further up the call stack
            //    }
            //    finally
            //    {
            //        db?.Close(); db?.Dispose();
            //    }
            //}

            if (string.IsNullOrWhiteSpace(CONNECTION_STR))
            {
                // سعی کن دوباره از Registry بخونی
                var dbms = new CL_CCNNMANAGER();
                if (!dbms.CheckIsConnectedToSQLDB())
                {
                    var ex = new InvalidOperationException("Cannot connect to database");
                    LogSqlQuery(sql, ex);
                    throw ex;  // خطای واضح و قابل فهم
                }
            }

            const int maxRetries = 3;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                using var db = new SqlConnection(CONNECTION_STR);
                try
                {
                    db.Open();
                    var results = db.Query<TEntity>(sql, parameters, commandTimeout: 3600);
                    return results;
                }
                //catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                catch (SqlException ex) when ((ex.Number == 1205 || (IsConnectionRelated(ex) && !IsNonRetriableAuthenticationError(ex))) && attempt < maxRetries)
                {
                    Thread.Sleep(200 * (attempt + 1));
                    continue;
                }
                catch (SqlException sqlEx)
                {
                    if (IsConnectionRelated(sqlEx))
                    {
                        ConnectedToSQLDB = false;
                    }

                    LogSqlQuery(sql, sqlEx);

                    throw;
                }
                catch (Exception er)
                {
                    LogSqlQuery(sql, er);

                    throw; // Re-throw the exception to handle it further up the call stack
                }
                finally
                {
                    db?.Close(); db?.Dispose();
                }
            }

            return null;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public int? DoExecuteSQL(string sql, object parameters = null)
        {
            if (string.IsNullOrWhiteSpace(CONNECTION_STR))
            {
                // سعی کن دوباره از Registry بخونی
                var dbms = new CL_CCNNMANAGER();
                if (!dbms.CheckIsConnectedToSQLDB())
                {
                    var ex = new InvalidOperationException("Cannot connect to database");
                    LogSqlQuery(sql, ex);
                    throw ex;  // خطای واضح و قابل فهم
                }
            }

            //using var db = new SqlConnection(CONNECTION_STR);

            int maxRetries = 3;
            for (int i = 0; i <= maxRetries; i++)
            {
                using var db = new SqlConnection(CONNECTION_STR);
                try
                {
                    db.Open();
                    var result = db.Execute(sql, parameters, commandTimeout: 3600);
                    return result;
                }
                catch (SqlException ex)
                {
                    if ((ex.Number == 1205 || (IsConnectionRelated(ex) && !IsNonRetriableAuthenticationError(ex))) && i < maxRetries) // 1205 = Deadlock or transient connection error
                    {
                        System.Threading.Thread.Sleep(200 * (i + 1)); // Wait and retry
                        continue;
                    }
                    if (IsConnectionRelated(ex))
                    {
                        ConnectedToSQLDB = false;
                    }
                    throw; // Rethrow if not deadlock or max retries reached
                }
                catch (Exception er)
                {
                    LogSqlQuery(sql, er);
                    throw; // Re-throw the exception to handle it further up the call stack
                }
                finally
                {
                    db?.Close(); db?.Dispose();
                }
            }

            return null;
        }

        private static void LogSqlQuery(string sql, Exception er)
        {
            try
            {
                File.AppendAllText(DbmsFullPathFile, $"\n\n------------------------------------------------------------" +
                         $"\n {DateTime.Now} {Baseknow.UUSER} \n" +
                         $" Error in DoExecuteSQL :[  {sql}  ]\n" +
                         $"{er.Message} \n {er.InnerException} \n {er.StackTrace} \n {er.Source} \n" +
                         $"\n Method Name: {er.TargetSite.Name} \n Base Exception: {er.GetBaseException().Message} \n Exception Data: {er.Data}" +
                         $"\n Help Link: {er.HelpLink} \n  ExceptionType: {er.GetType().FullName} \n" + $"{CL_CCNNMANAGER.CONNECTION_STR}");
            }
            catch { }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public async Task<int?> DoExecuteSQLAsync(string sql, object parameters = null)
        {
            using (var db = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    await db.OpenAsync();
                    var result = await db.ExecuteAsync(sql, parameters, commandTimeout: 3600);
                    return result;
                }
                catch (SqlException sqlEx)
                {
                    if (IsConnectionRelated(sqlEx))
                    {
                        ConnectedToSQLDB = false;
                    }
                    await LogSqlQueryAsync(sql, sqlEx);
                    throw;
                }
                catch (Exception er)
                {
                    Console.WriteLine("Error in DoExecuteSQLAsync: " + er.Message + sql);
                    try
                    {
                        await File.AppendAllTextAsync("\n\n" + DbmsFullPathFile,
                            $"\n {DateTime.Now}  \n Error in DoExecuteSQLAsync :[  {sql}  ]\n" +
                            $"{er.Message} \n {er.InnerException} \n {er.StackTrace} \n {er.Source} \n" +
                            $"\n Method Name: {er.TargetSite.Name} \n Base Exception: {er.GetBaseException().Message} \n Exception Data: {er.Data}" +
                            $"\n Help Link: {er.HelpLink} \n  ExceptionType: {er.GetType().FullName} \n" + $"{CL_CCNNMANAGER.CONNECTION_STR}");
                    }
                    catch { }
                    throw; // Re-throw the exception to handle it further up the call stack
                }
                finally
                {
                    if (db.State == ConnectionState.Open)
                    {
                        await db.CloseAsync();
                    }
                    await db.DisposeAsync();

                }
            }
        }
        //Safe {↓
        public IEnumerable<TEntity> DoGetDataSQL_Safe<TEntity>(string sql, object parameters = null)
        {
            using (IDbConnection db = new SqlConnection(CONNECTION_STR))
            {
                //db.Open();
                using (var transaction = db.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        var commandDefinition = new CommandDefinition(sql, parameters: parameters, commandTimeout: 300);
                        var results = db.Query<TEntity>(commandDefinition);
                        transaction.Commit();
                        db?.Close();
                        return results;
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
                return null;
            }
        }

        //Safe ↑}
        //var rowsAffected = dbms.DoExecuteSQL("UPDATE MyTable SET Column1 = @value WHERE Id = @id", new { value = "NewValue", id = 1 });

        //Asyncronize{↓
        public async Task<IEnumerable<TEntity>> DoGetDataSQLAsync2<TEntity>(string sql, object parameters = null)
        {
            using (SqlConnection db = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    await db.OpenAsync().ConfigureAwait(false);
                    //var commandDefinition = new CommandDefinition(sql, parameters, commandTimeout: 300, flags: CommandFlags.Buffered);
                    return await db.QueryAsync<TEntity>(sql, parameters, commandTimeout: 3600).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw; // Consider handling the exception based on your use-case
                }
            }
        }
        public async Task ExecuteStoredProcedureAsync(string storedProcedureName, object parameters = null)
        {
            using (SqlConnection db = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    await db.OpenAsync().ConfigureAwait(false);
                    await db.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure, commandTimeout: 3600).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw; // Consider handling the exception based on your use-case
                }
                finally
                {
                    db?.Close();
                }
            }
        }
        public async Task<IEnumerable<TEntity>> DoGetDataSQLAsync<TEntity>(string sql, object? parameters = null)
        {
            using (SqlConnection db = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    await db.OpenAsync();
                    await db.ExecuteAsync("SET ARITHABORT ON");
                    var results = await db.QueryAsync<TEntity>(sql, parameters, commandTimeout: 3600);
                    //await db.ExecuteAsync("SET ARITHABORT OFF");
                    return results;
                }
                catch
                {
                    throw; // Rethrow the exception to be handled by the caller
                }
                finally
                {
                    db?.Close();
                }
            }
        }

        //Asyncronize}↑
        public void OpenStoredProcedure(string storedProcedureName, object parameters = null)
        {
            using (IDbConnection db = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    db?.Open();
                    var commandDefinition = new CommandDefinition(storedProcedureName, parameters: parameters, commandType: CommandType.StoredProcedure, commandTimeout: 3600);
                    db.Execute(commandDefinition);
                }
                catch (Exception)
                {
                }
                finally
                {
                    db?.Close();
                }
            }
        }


        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerable<TEntity> DoGetStoreProcedureSQL<TEntity>(string sql, object parameters = null)
        {
            using (SqlConnection db = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    db.Open();
                    var results = db.Query<TEntity>(sql, parameters, commandTimeout: 3600, commandType: System.Data.CommandType.StoredProcedure);
                    return results;
                }
                catch (Exception er)
                {
                    LogSqlQuery(sql, er);

                    throw; // Re-throw the exception to handle it further up the call stack
                }
                finally
                {
                    db?.Close(); db?.Dispose();
                }
            }
            return null;
        }

        #region CompleteAsync
        /// <summary>
        /// نسخه جدید و بهبودیافته برای اجرای کوئری‌های SELECT به صورت آسنکرون.
        /// این متد دارای منطق تلاش مجدد برای خطاهای گذرا و قابلیت لغو عملیات است.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public async Task<IEnumerable<TEntity>> SqlQueryAsync<TEntity>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
        {
            // این خط باعث می‌شود متد بلافاصله آزاد شود و UI قفل نشود
            // Defensive coding: Force continuation on a ThreadPool thread to mitigate deadlocks 
            // if the caller incorrectly blocks the UI thread (e.g., using .Result or .Wait()).
            await Task.Yield();

            const int maxRetries = 3;
            const int baseDelayMilliseconds = 200;

            // Transient SQL Server error codes worth retrying
            var transientErrorNumbers = new[] { 1205, -2, 4060, 40197, 40501, 40613, 49918, 49919, 49920 };

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                SqlConnection? db = null;
                try
                {
                    db = new SqlConnection(CONNECTION_STR);

                    // CRITICAL FIX: Pass CancellationToken to OpenAsync
                    await db.OpenAsync(cancellationToken).ConfigureAwait(false);

                    // CRITICAL FIX: Include cancellationToken in CommandDefinition
                    var command = new CommandDefinition(
                        commandText: sql,
                        parameters: parameters,
                        commandTimeout: 3600,
                        cancellationToken: cancellationToken);

                    var result = await db.QueryAsync<TEntity>(command).ConfigureAwait(false);

                    return result;
                }
                catch (SqlException ex) when (transientErrorNumbers.Contains(ex.Number) && attempt < maxRetries)
                {
                    // Transient error - retry with exponential backoff
                    var delay = baseDelayMilliseconds * (attempt + 1);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    // Loop continues to next attempt
                }
                catch (SqlException sqlEx)
                {
                    if (IsConnectionRelated(sqlEx))
                    {
                        ConnectedToSQLDB = false;
                    }
                    await LogSqlQueryAsync(sql, sqlEx);
                    throw;
                }
                catch (OperationCanceledException)
                {
                    // Operation was cancelled - log and rethrow
                    await LogSqlQueryAsync(sql, new Exception("Query cancelled by user or timeout"));
                    throw;
                }
                catch (Exception ex)
                {
                    // Non-transient error - log and rethrow
                    await LogSqlQueryAsync(sql, ex);
                    throw;
                }
                finally
                {
                    // Explicit disposal (await using doesn't work well with retry loops)
                    if (db != null)
                    {
                        await db.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }

            // All retries exhausted - return empty (or throw exception based on your requirements)
            await LogSqlQueryAsync(sql, new Exception($"Max retries ({maxRetries}) exceeded"));
            return Enumerable.Empty<TEntity>();
        }
        /// <summary>
        /// نسخه جدید و بهبودیافته برای اجرای دستورات SQL (INSERT, UPDATE, DELETE) به صورت آسنکرون.
        /// این متد دارای منطق تلاش مجدد برای خطاهای گذرا و قابلیت لغو عملیات است.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public async Task<int> ExecuteSqlCommandAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
        {
            // این خط باعث می‌شود متد بلافاصله آزاد شود و UI قفل نشود
            // Defensive coding: Force continuation on a ThreadPool thread to mitigate deadlocks 
            // if the caller incorrectly blocks the UI thread (e.g., using .Result or .Wait()).
            await Task.Yield();

            const int maxRetries = 3;
            const int baseDelayMilliseconds = 200;
            var transientErrorNumbers = new[] { 1205, -2, 4060, 40197, 40501, 40613, 49918, 49919, 49920 };

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                SqlConnection? db = null;
                try
                {
                    // بررسی لغو قبل از شروع هر تلاش
                    cancellationToken.ThrowIfCancellationRequested();

                    db = new SqlConnection(CONNECTION_STR);

                    // FIX: Pass CancellationToken to OpenAsync
                    await db.OpenAsync(cancellationToken).ConfigureAwait(false);

                    // FIX: Include cancellationToken in CommandDefinition
                    var command = new CommandDefinition(
                        commandText: sql,
                        parameters: parameters,
                        commandTimeout: 3600,
                        cancellationToken: cancellationToken);

                    var rowsAffected = await db.ExecuteAsync(command).ConfigureAwait(false);

                    return rowsAffected;
                }
                catch (SqlException ex) when (transientErrorNumbers.Contains(ex.Number) && attempt < maxRetries)
                {
                    // خطای گذرا - تلاش مجدد با تاخیر فزاینده
                    var delay = baseDelayMilliseconds * (attempt + 1);

                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // اگر در حین تاخیر لغو شد، exception را پرتاب کن
                        await LogSqlQueryAsync(sql, new Exception($"Command cancelled during retry attempt {attempt + 1}"));
                        throw;
                    }
                    // ادامه به تلاش بعدی
                }
                catch (SqlException sqlEx)
                {
                    if (IsConnectionRelated(sqlEx))
                    {
                        ConnectedToSQLDB = false;
                    }
                    await LogSqlQueryAsync(sql, sqlEx);
                    throw;
                }
                catch (OperationCanceledException)
                {
                    // عملیات توسط کاربر یا timeout لغو شد
                    await LogSqlQueryAsync(sql, new Exception("Command execution cancelled"));
                    throw;
                }
                catch (Exception ex)
                {
                    // خطای غیر گذرا - ثبت و پرتاب مجدد
                    await LogSqlQueryAsync(sql, ex);
                    throw;
                }
                finally
                {
                    // FIX: Explicit disposal to ensure connection is returned to pool
                    if (db != null)
                    {
                        await db.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }

            // اگر تمام تلاش‌ها ناموفق بود
            var finalError = new Exception($"Failed to execute SQL command after {maxRetries} retries");
            await LogSqlQueryAsync(sql, finalError);

            // FIX: Throw exception instead of returning -1 for consistency
            throw finalError;
        }

        private static async Task LogSqlQueryAsync(string sql, Exception er)
        {
            try
            {
                // اگر مسیر فایل موجود نیست، ایجاد کن
                string? directory = Path.GetDirectoryName(DbmsFullPathFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string logContent = $"\n\n------------------------------------------------------------" +
                                    $"\n {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} User: {Baseknow.UUSER} (Async) \n" +
                                    $" Error in Async SQL Operation: {sql?.Substring(0, Math.Min(sql.Length, 500))} \n" + // محدود کردن طول SQL
                                    $" Message: {er.Message} \n" +
                                    $" InnerException: {er.InnerException?.Message} \n" +
                                    $" StackTrace: {er.StackTrace} \n" +
                                    $" Source: {er.Source} \n" +
                                    $" Method: {er.TargetSite?.Name} \n" +
                                    $" ExceptionType: {er.GetType().FullName} \n";

                await File.AppendAllTextAsync(DbmsFullPathFile, logContent, Encoding.UTF8).ConfigureAwait(false);
            }
            catch
            {
                // Silent fail - logging should never crash the app
            }
        }
        #endregion

        public List<string> GetSQLServerList()
        {
            List<string> serverList = new List<string>();
            DataTable servers = SqlDataSourceEnumerator.Instance.GetDataSources();
            foreach (DataRow server in servers.Rows)
            {
                string serverName = server["ServerName"].ToString();
                string instanceName = server["InstanceName"].ToString();

                // If InstanceName is empty, then it's a default (unnamed) instance, 
                // else it's a named instance
                string fullServerName = string.IsNullOrEmpty(instanceName) ? serverName : $"{serverName}\\{instanceName}";

                serverList.Add(fullServerName);
            }
            return serverList;
        }
        public string GetConnectionSpecifyNameApp()
        {
            using (RegistryKey MainKeyPath = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BDGT", true))
            {
                string CC = "";
                try
                {
                    if (MainKeyPath != null)
                    {
                        if (!(MainKeyPath.GetSubKeyNames().FirstOrDefault() is null))
                        {
                            foreach (string Keyname in MainKeyPath.GetSubKeyNames())
                            {
                                if (Keyname.ToLower() == CL_Generaly.MyExeFileName.ToLower()) // Found Folder
                                {
                                    using (RegistryKey key = MainKeyPath.OpenSubKey(Keyname))
                                    {
                                        object okey = key.GetValue("CreateConnection", null);
                                        if (okey is null)
                                        {
                                            CC = "";
                                        }
                                        else
                                        {
                                            CC = okey.ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //CONNECTION_STR = CL_CryptionAlgorithem.DecryptTextUsingUTF8(CC);
                }
                catch (Exception)
                {
                    //Msgwin msgwin = new Msgwin(false, "خطا در بررسی اولیه اتصال به سرور !"); msgwin.ShowDialog();
                    //MessageBox.Show("خطا در بررسی اولیه اتصال به سرور !");
                    CC = "InitError";
                }
                finally
                {
                    if (!ReferenceEquals(MainKeyPath, null))
                    {
                        MainKeyPath.Close();
                    }
                }
                return CC;
            }
        }
        public bool CreateConnectionSpecifyNameApp(string entityConnectionString)
        {
            using (RegistryKey MainKeyPath = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BDGT", true))
            {
                try
                {
                    if (MainKeyPath != null)
                    {
                        //Exist
                        if (!(MainKeyPath.GetSubKeyNames().FirstOrDefault() is null))
                        {
                            foreach (string Keyname in MainKeyPath.GetSubKeyNames())
                            {
                                if (Keyname == CL_Generaly.MyExeFileName) // Found Folder
                                {
                                    using (RegistryKey key = MainKeyPath.OpenSubKey(Keyname, true))
                                    {
                                        var CC = key.GetValue("CreateConnection", null);
                                        if (CC == null)
                                        {
                                            //اگر خالی هست مسیر رو بساز
                                            key.SetValue("CreateConnection", CL_CryptionAlgorithem.EncryptTextUsingUTF8(entityConnectionString));
                                        }
                                        else
                                        {
                                            key.SetValue("CreateConnection", CL_CryptionAlgorithem.EncryptTextUsingUTF8(entityConnectionString));
                                        }
                                    }
                                }
                                else
                                {
                                    //Create Folder by exe name for first time
                                    using (RegistryKey ConnectionKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\BDGT\\{CL_Generaly.MyExeFileName}"))
                                    {
                                        ConnectionKey.SetValue("CreateConnection", CL_CryptionAlgorithem.EncryptTextUsingUTF8(entityConnectionString));
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Create Folder by exe name for first time
                            using (RegistryKey ConnectionKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\BDGT\\{CL_Generaly.MyExeFileName}"))
                            {
                                ConnectionKey.SetValue("CreateConnection", CL_CryptionAlgorithem.EncryptTextUsingUTF8(entityConnectionString));
                            }
                        }
                    }
                    else
                    {
                        //Not Exist
                        //Create Folder by exe name for first time
                        using (RegistryKey ConnectionKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\BDGT\\{CL_Generaly.MyExeFileName}"))
                        {
                            ConnectionKey.SetValue("CreateConnection", CL_CryptionAlgorithem.EncryptTextUsingUTF8(entityConnectionString));
                        }
                    }

                    CONNECTION_STR = entityConnectionString;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    if (!ReferenceEquals(MainKeyPath, null))
                        MainKeyPath.Close();
                }
                return true;
            }
        }
        public bool CheckIsConnectedToSQLDB()
        {
            var RegConnectionStr = "";
            try
            {
                RegConnectionStr = GetConnectionSpecifyNameApp();

                if (RegConnectionStr == "InitError" || string.IsNullOrWhiteSpace(RegConnectionStr))
                {
                    CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    return false;
                }
                if (RegConnectionStr == "")
                {
                    CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    return false;
                }
                else
                {
                    //CONNECTION_STR = CL_CryptionAlgorithem.DecryptTextUsingUTF8(RegConnectionStr) + "TrustServerCertificate=True;";

                    var builder = new SqlConnectionStringBuilder(CL_CryptionAlgorithem.DecryptTextUsingUTF8(RegConnectionStr));
                    builder.TrustServerCertificate = true;
                    builder.MultipleActiveResultSets = true;
                    builder.MaxPoolSize = 1000;
                    CONNECTION_STR = builder.ConnectionString + ";";

                    CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

                    using var db = new SqlConnection(CONNECTION_STR);
                    db.Open();
                    var results = db.Query<string>("SELECT SERVERNAM FROM dbo.SAZMAN").FirstOrDefault();

                    //dbms.Databasek.CommandTimeout = 100;
                    var TestCnn = dbms.DoGetDataSQL<string>("SELECT SERVERNAM FROM dbo.SAZMAN").FirstOrDefault();

                    db?.Close(); db?.Dispose();
                    CL_CCNNMANAGER.ConnectedToSQLDB = true;
                    return true;
                }
            }
            catch (Exception)
            {
                //ممکن است دیتابیس مال این نرم افزار نباشد یا مدل داده ایش فرق کرده باشه یعنی تیبل مورد نظر تغییر کرده که توی سیشارپ هم باید آپدیت بشه
                //Msgwin msgwin = new Msgwin(false, "ممکن هست این دیتابیس برای من نباشد !"); msgwin.ShowDialog();
                //Choosing_Connection choscnn = new Choosing_Connection();
                //choscnn.ShowDialog();
                CL_CCNNMANAGER.ConnectedToSQLDB = false;
                return false;
            }
        }
        public static async Task<bool> IsAvailableAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CONNECTION_STR)) return false;
                var builder = new SqlConnectionStringBuilder(CONNECTION_STR);
                builder.ConnectTimeout = 3; // 3 seconds timeout
                using var db = new SqlConnection(builder.ConnectionString);
                await db.OpenAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
