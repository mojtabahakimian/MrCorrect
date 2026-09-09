using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;

namespace Prg_Proccessy.FUNCTIONS
{
    public static class CL_AuditEngine
    {
        private static readonly Channel<AuditEntryModel> _channel;
        private static readonly Guid _sessionId = Guid.NewGuid();
        private static readonly string _localIp = GetLocalIpAddress();
        private static readonly string _machineName = Environment.MachineName;

        static CL_AuditEngine()
        {
            var options = new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true
            };
            _channel = Channel.CreateBounded<AuditEntryModel>(options);

            Task.Factory.StartNew(ProcessQueueAsync, TaskCreationOptions.LongRunning);
        }

        private static string GetLocalIpAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(string actionType, string actionCaption, string moduleTitle,
                               string formClass, double? docNumber = null, double? docTag = null,
                               string? detailsJson = null, byte severity = 1, string? entityName = null)
        {
            try
            {
                var now = DateTime.Now;
                var pc = new PersianCalendar();
                long pDate = (long)pc.GetYear(now) * 10000 + pc.GetMonth(now) * 100 + pc.GetDayOfMonth(now);

                var entry = new AuditEntryModel
                {
                    PersianDate = pDate,
                    LogTime = now.ToString("HH:mm:ss"),
                    UserId = Baseknow.USERCOD ?? 0,
                    UserName = Baseknow.UUSER ?? string.Empty,
                    ClientIP = _localIp,
                    MachineName = _machineName,
                    SessionId = _sessionId,
                    ModuleTitle = moduleTitle ?? string.Empty,
                    FormClass = formClass ?? string.Empty,
                    ActionType = actionType ?? string.Empty,
                    ActionCaption = actionCaption ?? string.Empty,
                    EntityName = entityName ?? (docTag.HasValue ? "HEAD_LST" : null),
                    DocNumber = docNumber,
                    DocTag = docTag,
                    DetailsJson = detailsJson,
                    Severity = severity
                };

                _channel.Writer.TryWrite(entry);
            }
            catch
            {
                // Engine write failure must never crash UI
            }
        }

        private static async Task ProcessQueueAsync()
        {
            var buffer = new List<AuditEntryModel>(100);

            while (await _channel.Reader.WaitToReadAsync())
            {
                buffer.Clear();
                var deadline = DateTime.UtcNow.AddMilliseconds(1000);

                while (buffer.Count < 100 && DateTime.UtcNow < deadline && _channel.Reader.TryRead(out var item))
                {
                    buffer.Add(item);
                }

                if (buffer.Count == 0) continue;

                try
                {
                    using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        await db.OpenAsync();
                        const string sql = @"
                            INSERT INTO dbo.SYS_AUDIT_TRAIL
                            (PersianDate, LogTime, UserId, UserName, ClientIP, MachineName, SessionId,
                             ModuleTitle, FormClass, ActionType, ActionCaption, EntityName, DocNumber, DocTag, DetailsJson, Severity)
                            VALUES
                            (@PersianDate, @LogTime, @UserId, @UserName, @ClientIP, @MachineName, @SessionId,
                             @ModuleTitle, @FormClass, @ActionType, @ActionCaption, @EntityName, @DocNumber, @DocTag, @DetailsJson, @Severity);";

                        await db.ExecuteAsync(sql, buffer);
                    }
                }
                catch
                {
                    // Logging execution failure must never throw
                }
            }
        }
    }
}
