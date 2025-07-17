using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Functions
{
    public class AuditLogEntry
    {
        public string UserName { get; set; }
        public string WindowsUserName { get; set; }
        public string ActionType { get; set; }
        public string TableName { get; set; }
        public string RecordID { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string IPAddress { get; set; }
        public string MachineName { get; set; }
        public string ApplicationVersion { get; set; }
        public string WindowsVersion { get; set; }
        public DateTime ActionDateTime { get; set; }
        public string AdditionalInfo { get; set; }
        public Guid SessionID { get; set; }
        public int ProcessID { get; set; }
        public int ThreadID { get; set; }
        public string StackTrace { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
    public static class AuditLogger
    {
        private static readonly CL_CCNNMANAGER _dbms = new CL_CCNNMANAGER();
        private static readonly Guid _sessionId = Guid.NewGuid();
        private static readonly string _appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static object _lockObject = new object();

        private static readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        public static async Task LogActionAsync(string actionType, string tableName, string recordId, string oldValue = null, string newValue = null, string additionalInfo = null)
        {
            try
            {
                var entry = CreateAuditEntry(actionType, tableName, recordId, oldValue, newValue, additionalInfo);
                await SaveAuditLogAsync(entry);
            }
            catch (Exception ex)
            {
                //// Fallback logging to file system if database logging fails
                //await FallbackLogToFileAsync(ex, actionType, tableName, recordId);
            }
        }

        private static AuditLogEntry CreateAuditEntry(string actionType, string tableName, string recordId, string oldValue, string newValue, string additionalInfo)
        {
            var ipAddresses = GetIPAddresses();
            var windowsVersion = GetWindowsVersion();

            return new AuditLogEntry
            {
                UserName = GetCurrentUser(),
                WindowsUserName = Environment.UserName,
                ActionType = actionType,
                TableName = tableName,
                RecordID = recordId,
                OldValue = oldValue,
                NewValue = newValue,
                IPAddress = string.Join(";", ipAddresses),
                MachineName = Environment.MachineName,
                ApplicationVersion = CL_VERSION.MrCorrectFullVersion,
                WindowsVersion = windowsVersion,
                ActionDateTime = DateTime.Now,
                AdditionalInfo = additionalInfo,
                SessionID = _sessionId,
                ProcessID = Process.GetCurrentProcess().Id,
                ThreadID = Thread.CurrentThread.ManagedThreadId,
                //StackTrace = new StackTrace(true).ToString(),
                StackTrace = null,
                IsSuccess = true
            };
        }

        private static async Task SaveAuditLogAsync(AuditLogEntry entry)
        {
            try
            {
                await _asyncLock.WaitAsync(); // Use SemaphoreSlim instead of lock for async operations

                var sql = @"
                 INSERT INTO USER_AUDIT_LOG (
                     UserName, WindowsUserName, ActionType, TableName, RecordID, 
                     OldValue, NewValue, IPAddress, MachineName, ApplicationVersion,
                     WindowsVersion, ActionDateTime, AdditionalInfo, SessionID,
                     ProcessID, ThreadID, StackTrace, IsSuccess, ErrorMessage
                 )
                 VALUES (
                     @UserName, @WindowsUserName, @ActionType, @TableName, @RecordID,
                     @OldValue, @NewValue, @IPAddress, @MachineName, @ApplicationVersion,
                     @WindowsVersion, @ActionDateTime, @AdditionalInfo, @SessionID,
                     @ProcessID, @ThreadID, @StackTrace, @IsSuccess, @ErrorMessage
                 )";

                await _dbms.DoExecuteSQLAsync(sql, entry);
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        private static string GetCurrentUser()
        {
            return string.IsNullOrEmpty(Baseknow.UUSER) ? "NOTPASSED" : Baseknow.UUSER; // Replace with actual implementation
        }

        private static List<string> GetIPAddresses()
        {
            var ipAddresses = new List<string>();
            try
            {
                var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                ipAddresses.AddRange(hostEntry.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString()));
            }
            catch
            {
                ipAddresses.Add("Unable to determine IP");
            }
            return ipAddresses;
        }

        private static string GetWindowsVersion()
        {
            try
            {
                return Environment.OSVersion.VersionString;
            }
            catch
            {
                return "Unknown";
            }
        }

        private static async Task FallbackLogToFileAsync(Exception ex, string actionType, string tableName, string recordId)
        {
            var fallbackLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MCR_DEL", "FallbackLogs");

            Directory.CreateDirectory(fallbackLogPath);

            var logFile = Path.Combine(fallbackLogPath, $"AuditLog_{DateTime.Now:yyyyMMdd}.txt");

            var logEntry = $@"
                              Time: {DateTime.Now}
                              Error: Database logging failed
                              Action: {actionType}
                              Table: {tableName}
                              RecordID: {recordId}
                              Exception: {ex}
                              User: {Environment.UserName}
                              Machine: {Environment.MachineName}
                              ----------------------------------------";

            await File.AppendAllTextAsync(logFile, logEntry);
        }
    }

}
