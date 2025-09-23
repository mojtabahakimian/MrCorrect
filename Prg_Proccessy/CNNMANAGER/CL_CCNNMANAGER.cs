using Dapper;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using System.Data;
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
        public static string CONNECTION_STR { get; set; } // = @"Data Source=MERCEDES\SQL2008;Initial Catalog=YAZD2024;Integrated Security=True;TrustServerCertificate=True;";
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

            // If the password was not provided, and we're not using Integrated Security, set it as an empty string.
            if (!passwordProvided && userIdProvided && !integratedSecurityProvided)
            {
                connectionStringBuilder.Password = string.Empty;
            }

            return connectionStringBuilder.ConnectionString + ";";
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
                catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                {
                    Thread.Sleep(200 * (attempt + 1));
                    continue;
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
            using var db = new SqlConnection(CONNECTION_STR);

            int maxRetries = 3;
            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    db.Open();
                    var result = db.Execute(sql, parameters, commandTimeout: 3600);
                    return result;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205 && i < maxRetries) // 1205 = Deadlock
                    {
                        System.Threading.Thread.Sleep(200 * (i + 1)); // Wait and retry
                        continue;
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
        public int? DoExecuteSQL_Safe(string sql, object parameters = null)
        {
            using (IDbConnection db = new SqlConnection(CONNECTION_STR))
            {
                //db.Open();
                using (var transaction = db.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        var commandDefinition = new CommandDefinition(sql, parameters: parameters, commandTimeout: 300);
                        var results = db.Execute(commandDefinition);
                        transaction.Commit();
                        //return db.Execute(sql, parameters);
                        return results;
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                    db?.Close();
                }
            }
            return null;
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

                if (RegConnectionStr == "InitError")
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
                    CONNECTION_STR = CL_CryptionAlgorithem.DecryptTextUsingUTF8(RegConnectionStr) + "TrustServerCertificate=True;";
                    CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

                    //dbms.Databasek.CommandTimeout = 100;
                    var TestCnn = dbms.DoGetDataSQL<string>("SELECT SERVERNAM FROM dbo.SAZMAN").FirstOrDefault();
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

    }
}
