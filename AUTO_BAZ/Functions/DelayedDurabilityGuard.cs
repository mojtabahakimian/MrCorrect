using Microsoft.Data.SqlClient;
using Prg_SendInvoice.CNNMANAGER;
using System;

namespace AUTO_BAZ.Functions
{
    /// <summary>
    /// مدیریت موقت DELAYED_DURABILITY برای دیتابیس‌های مشخص در طول اجرای بازسازی.
    /// </summary>
    public static class DelayedDurabilityGuard
    {
        private const string ForceSql = @"
ALTER DATABASE YAZDSEPAR1404 SET DELAYED_DURABILITY = FORCED;
ALTER DATABASE YAZDSEPAR1405 SET DELAYED_DURABILITY = FORCED;";

        private const string DisableSql = @"
ALTER DATABASE YAZDSEPAR1404 SET DELAYED_DURABILITY = DISABLED;
ALTER DATABASE YAZDSEPAR1405 SET DELAYED_DURABILITY = DISABLED;";

        private static readonly object SyncObj = new object();
        private static int _activeScopes = 0;
        private static bool _isForced = false;

        public static void EnterRebuildScope()
        {
            lock (SyncObj)
            {
                _activeScopes++;
                if (_activeScopes > 1 || _isForced)
                {
                    return;
                }

                ExecuteAgainstMaster(ForceSql);
                _isForced = true;
                CL_LMethods.LogWriter.WriteLog("DELAYED_DURABILITY => FORCED (YAZDSEPAR1404, YAZDSEPAR1405)");
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
                    ExecuteAgainstMaster(DisableSql);
                    CL_LMethods.LogWriter.WriteLog("DELAYED_DURABILITY => DISABLED (YAZDSEPAR1404, YAZDSEPAR1405)");
                }
                finally
                {
                    _isForced = false;
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
                if (!_isForced)
                {
                    return;
                }

                try
                {
                    ExecuteAgainstMaster(DisableSql);
                    CL_LMethods.LogWriter.WriteLog("Emergency DELAYED_DURABILITY rollback => DISABLED");
                }
                catch (Exception ex)
                {
                    CL_LMethods.ExpectionLogWriter.WriteLog(ex, "TryDisableForcefully in DelayedDurabilityGuard");
                }
                finally
                {
                    _activeScopes = 0;
                    _isForced = false;
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
    }
}
