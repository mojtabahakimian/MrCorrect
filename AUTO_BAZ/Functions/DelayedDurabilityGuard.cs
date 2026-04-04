using Dapper;
using Microsoft.Data.SqlClient;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Linq;

namespace AUTO_BAZ.Functions
{
    /// <summary>
    /// مدیریت موقت DELAYED_DURABILITY برای دیتابیس جاری در طول اجرای بازسازی.
    /// </summary>
    public static class DelayedDurabilityGuard
    {
        private static readonly object SyncObj = new object();
        private static int _activeScopes = 0;
        private static bool _isForced = false;
        private static string? _targetDbName = null;

        public static void EnterRebuildScope()
        {
            lock (SyncObj)
            {
                _activeScopes++;
                if (_activeScopes > 1 || _isForced)
                {
                    return;
                }

                _targetDbName = GetCurrentDatabaseName();
                ExecuteAgainstMaster(BuildAlterDelayedDurabilitySql(_targetDbName, forced: true));
                _isForced = true;
                CL_LMethods.LogWriter.WriteLog($"DELAYED_DURABILITY => FORCED ({_targetDbName})");
            }
        }

        public static void ExitRebuildScope()
        {
            lock (SyncObj)
            {
                if (_activeScopes > 0)
                {
                    _activeScopes--;
                }

                if (_activeScopes > 0 || !_isForced)
                {
                    return;
                }

                try
                {
                    ExecuteAgainstMaster(BuildAlterDelayedDurabilitySql(_targetDbName ?? GetCurrentDatabaseName(), forced: false));
                    CL_LMethods.LogWriter.WriteLog($"DELAYED_DURABILITY => DISABLED ({_targetDbName})");
                }
                finally
                {
                    _isForced = false;
                    _targetDbName = null;
                }
            }
        }

        /// <summary>
        /// برای سناریوهای خروج اضطراری مثل Crash/Exit.
        /// </summary>
        public static void TryDisableForcefully()
        {
            lock (SyncObj)
            {
                try
                {
                    using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
                    db.Open();
                    string? DelayedStateValue = db.Query<string?>("SELECT delayed_durability_desc FROM sys.databases WHERE name = DB_NAME()", default).FirstOrDefault();
                    if (DelayedStateValue != null)
                    {
                        _isForced = DelayedStateValue.Equals("FORCED", StringComparison.CurrentCultureIgnoreCase);
                    }
                }
                catch (Exception er)
                {
                    if (!_isForced)
                    {
                        return;
                    }
                }

                try
                {
                    var dbName = _targetDbName ?? GetCurrentDatabaseName();
                    ExecuteAgainstMaster(BuildAlterDelayedDurabilitySql(dbName, forced: false));
                    CL_LMethods.LogWriter.WriteLog($"Emergency DELAYED_DURABILITY rollback => DISABLED ({dbName})");
                }
                catch (Exception ex)
                {
                    CL_LMethods.ExpectionLogWriter.WriteLog(ex, "TryDisableForcefully in DelayedDurabilityGuard");
                }
                finally
                {
                    _activeScopes = 0;
                    _isForced = false;
                    _targetDbName = null;
                }
            }
        }

        private static void ExecuteAgainstMaster(string sql)
        {
            var current = CL_CCNNMANAGER.CONNECTION_STR;
            if (string.IsNullOrWhiteSpace(current))
            {
                throw new InvalidOperationException("CONNECTION_STR is empty.");
            }

            var builder = new SqlConnectionStringBuilder(current)
            {
                InitialCatalog = "master"
            };

            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn)
            {
                CommandTimeout = 60
            };
            cmd.ExecuteNonQuery();
        }

        private static string GetCurrentDatabaseName()
        {
            var current = CL_CCNNMANAGER.CONNECTION_STR;
            if (string.IsNullOrWhiteSpace(current))
            {
                throw new InvalidOperationException("CONNECTION_STR is empty.");
            }

            var builder = new SqlConnectionStringBuilder(current);
            var dbName = builder.InitialCatalog?.Trim();
            if (string.IsNullOrWhiteSpace(dbName))
            {
                throw new InvalidOperationException("Initial Catalog is missing in CONNECTION_STR.");
            }

            return dbName;
        }

        private static string BuildAlterDelayedDurabilitySql(string dbName, bool forced)
        {
            var escaped = dbName.Replace("]", "]]");
            var mode = forced ? "FORCED" : "DISABLED";
            return $"ALTER DATABASE [{escaped}] SET DELAYED_DURABILITY = {mode};";
        }
    }
}
