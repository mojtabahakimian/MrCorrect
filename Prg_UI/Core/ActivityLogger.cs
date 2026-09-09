using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Prg_Proccessy;

namespace Prg_UI.Core
{
    public class ActivityLogEntry
    {
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public string ActionType { get; set; }
        public string EntityName { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string MachineName { get; set; }
    }

    public class ActivityLogger : IDisposable
    {
        private static readonly Lazy<ActivityLogger> _instance = new Lazy<ActivityLogger>(() => new ActivityLogger());
        public static ActivityLogger Instance => _instance.Value;

        private readonly Channel<ActivityLogEntry> _channel;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _processTask;

        private string _machineName;
        private string _ipAddress;
        private string _macAddress;

        private ActivityLogger()
        {
            _channel = Channel.CreateUnbounded<ActivityLogEntry>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            _cancellationTokenSource = new CancellationTokenSource();
            _processTask = Task.Run(ProcessQueueAsync);

            Task.Run(InitializeClientInfo);
        }

        private void InitializeClientInfo()
        {
            try
            {
                _machineName = Environment.MachineName;

                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        if (nic.OperationalStatus == OperationalStatus.Up)
                        {
                            _macAddress = string.Join(":", nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));

                            var ipProps = nic.GetIPProperties();
                            var ipv4 = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                            if (ipv4 != null)
                            {
                                _ipAddress = ipv4.Address.ToString();
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public void Log(string actionType, string entityName, string description)
        {
            long? userId = null;
            if (long.TryParse(Baseknow.USERCOD, out long parsedUserId))
            {
                userId = parsedUserId;
            }

            var entry = new ActivityLogEntry
            {
                UserId = userId,
                UserName = Baseknow.UUSER,
                ActionType = actionType,
                EntityName = entityName,
                Description = description,
                IpAddress = _ipAddress,
                MacAddress = _macAddress,
                MachineName = _machineName
            };

            _channel.Writer.TryWrite(entry);
        }

        private async Task ProcessQueueAsync()
        {
            var reader = _channel.Reader;
            try
            {
                while (await reader.WaitToReadAsync(_cancellationTokenSource.Token))
                {
                    var entries = new List<ActivityLogEntry>();
                    while (reader.TryRead(out var entry))
                    {
                        entries.Add(entry);
                        if (entries.Count >= 50) break; // Batch size
                    }

                    if (entries.Count > 0)
                    {
                        await SaveToDatabaseAsync(entries);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Task cancelled
            }
        }

        private async Task SaveToDatabaseAsync(List<ActivityLogEntry> entries)
        {
            try
            {
                if (string.IsNullOrEmpty(CL_CCNNMANAGER.CONNECTION_STR)) return;

                using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    await db.OpenAsync(_cancellationTokenSource.Token);

                    var sql = @"INSERT INTO SYS_ACTIVITY_LOG
                                (UserId, UserName, ActionType, EntityName, Description, IpAddress, MacAddress, MachineName)
                                VALUES (@UserId, @UserName, @ActionType, @EntityName, @Description, @IpAddress, @MacAddress, @MachineName)";

                    await db.ExecuteAsync(sql, entries);
                }
            }


            catch (Exception ex)
            {
                try
                {
                    string logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                    if (!System.IO.Directory.Exists(logDir))
                    {
                        System.IO.Directory.CreateDirectory(logDir);
                    }
                    string logFile = System.IO.Path.Combine(logDir, $"ActivityLog_Error_{DateTime.Now:yyyyMMdd}.txt");

                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"{DateTime.Now}: Failed to save {entries.Count} logs. Error: {ex.Message}");
                    foreach(var entry in entries)
                    {
                        sb.AppendLine($"   [DROP] User:{entry.UserId} | Action:{entry.ActionType} | Entity:{entry.EntityName} | Desc:{entry.Description}");
                    }
                    System.IO.File.AppendAllText(logFile, sb.ToString());
                }
                catch { }
            }


        }

                public void Dispose()
        {
            _channel.Writer.Complete();
            try
            {
                // Give the background task some time to flush the remaining queue
                _processTask.Wait(TimeSpan.FromSeconds(3));
            }
            catch { }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
