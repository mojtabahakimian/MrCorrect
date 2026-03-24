using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Prg_UI.Functions
{
    public static class SqlServerScanner
    {
        /// <summary>
        /// لیست مرتب‌شدهٔ نام سرورهای SQL در شبکه و روی همین کامپیوتر.
        /// </summary>
        public static List<string> GetAllSqlServerNames(int udpTimeoutMs = 900)
        {
            var servers = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

            // ➊ UDP network scan and hostname scan run in parallel for speed
            var udpTask = Task.Run(() =>
            {
                foreach (var s in ScanUdpNetwork(udpTimeoutMs)) servers.TryAdd(s, 0);
            });

            var hostnameTask = Task.Run(() =>
            {
                foreach (var s in ScanCommonHostnames()) servers.TryAdd(s, 0);
            });

            // ➋ Local registry is instant – run on current thread while network scans run
            RegistryView[] views = Environment.Is64BitOperatingSystem
                ? new[] { RegistryView.Registry64, RegistryView.Registry32 }
                : new[] { RegistryView.Registry32 };

            foreach (var view in views)
                foreach (var s in GetSqlInstancesFromRegistry(view))
                    servers.TryAdd(s, 0);

            Task.WaitAll(udpTask, hostnameTask);

            return servers.Keys.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
        }

        // ---------------------------------------------------------------------------
        //                        UDP 1434 SCAN (NETWORK)
        // ---------------------------------------------------------------------------
        private const int SqlBrowserPort = 1434;

        private static IEnumerable<string> ScanUdpNetwork(int receiveTimeoutMs)
        {
            var results = new ConcurrentBag<string>();
            var request = new byte[] { 0x02 }; // SQL Server browser request

            // Collect all local IPv4 broadcast addresses
            var broadcasts = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses
                    .Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork && u.IPv4Mask != null)
                    .Select(u => new IPEndPoint(GetBroadcast(u.Address, u.IPv4Mask), SqlBrowserPort)))
                .Distinct(new IPEndPointComparer())
                .ToList();

            if (broadcasts.Count == 0)
                yield break;

            using (var udp = new UdpClient(AddressFamily.InterNetwork))
            {
                udp.EnableBroadcast = true;
                udp.Client.ReceiveTimeout = receiveTimeoutMs;

                // Send UDP broadcast on all networks
                foreach (var ep in broadcasts)
                {
                    try { udp.Send(request, request.Length, ep); }
                    catch { /* ignore */ }
                }

                var deadline = DateTime.UtcNow.AddMilliseconds(receiveTimeoutMs);
                while (DateTime.UtcNow < deadline)
                {
                    try
                    {
                        if (udp.Available == 0) { Thread.Sleep(40); continue; }
                        var remote = new IPEndPoint(IPAddress.Any, 0);
                        var resp = udp.Receive(ref remote);
                        if (resp.Length > 3)
                        {
                            foreach (var srv in ParseServerInfoList(resp, remote.Address))
                                results.Add(srv);
                        }
                    }
                    catch (SocketException) { break; } // timeout, done
                    catch { /* ignore */ }
                }
            }
            // Remove duplicates
            foreach (var s in results.Distinct(StringComparer.OrdinalIgnoreCase))
                yield return s;
        }

        // Handles multiple server instances in response (covers clusters/AG)
        // SQL Browser response format: byte[0]=0x05, bytes[1-2]=length (LE), bytes[3+]=data
        private static IEnumerable<string> ParseServerInfoList(byte[] payload, IPAddress ip)
        {
            if (payload.Length <= 3) yield break;

            // Skip the 3-byte header (0x05 + 2-byte length) to get the actual data
            var txt = Encoding.ASCII.GetString(payload, 3, payload.Length - 3);

            string server = null;
            var tokens = txt.Split(';');
            for (int i = 0; i < tokens.Length - 1; i++)
            {
                if (tokens[i].Equals("ServerName", StringComparison.OrdinalIgnoreCase))
                    server = tokens[i + 1];
                else if (tokens[i].Equals("InstanceName", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(server))
                {
                    var instance = tokens[i + 1];
                    if (instance.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                        yield return server;
                    else
                        yield return $"{server}\\{instance}";
                    server = null; // reset for next server block in multi-instance response
                }
            }
            // Fallback: some servers (default instance) might only return server name
            if (!tokens.Any(t => t.Equals("InstanceName", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(server))
                yield return server;
        }

        private static IPAddress GetBroadcast(IPAddress addr, IPAddress mask)
        {
            var a = addr.GetAddressBytes();
            var m = mask.GetAddressBytes();
            var b = new byte[4];
            for (int i = 0; i < 4; i++)
                b[i] = (byte)(a[i] | (~m[i]));
            return new IPAddress(b);
        }

        // ---------------------------------------------------------------------------
        //                        LOCAL REGISTRY INSTANCES
        // ---------------------------------------------------------------------------
        private static IEnumerable<string> GetSqlInstancesFromRegistry(RegistryView view)
        {
            const string regPath = @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL";
            var list = new List<string>();
            try
            {
                using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var key = hklm.OpenSubKey(regPath);
                if (key == null) return list;

                string machine = Environment.MachineName;
                foreach (var inst in key.GetValueNames())
                {
                    list.Add(inst.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
                        ? machine
                        : $"{machine}\\{inst}");
                }
            }
            catch { /* No registry access */ }
            return list;
        }

        // ---------------------------------------------------------------------------
        //        FAST TCP PORT CHECK FOR COMMON SQL SERVER HOSTNAMES
        //        Catches servers where SQL Browser is disabled (port 1433 is open)
        // ---------------------------------------------------------------------------
        private static IEnumerable<string> ScanCommonHostnames()
        {
            string[] hostnames = { "MAIN", "SQL", "SERVER", "DBSERVER", "PC229" };
            var found = new ConcurrentBag<string>();
            Parallel.ForEach(hostnames, hn =>
            {
                try
                {
                    using var client = new TcpClient();
                    // ConnectAsync (Task-based) is always completed by the using-dispose;
                    // Wait(ms) returns false on timeout so no resources are orphaned.
                    var connectTask = client.ConnectAsync(hn, 1433);
                    if (connectTask.Wait(300) && client.Connected)
                        found.Add(hn);
                }
                catch { }
            });
            return found;
        }

        // Helper for IP broadcast deduplication
        private class IPEndPointComparer : IEqualityComparer<IPEndPoint>
        {
            public bool Equals(IPEndPoint x, IPEndPoint y) => x.Address.Equals(y.Address) && x.Port == y.Port;
            public int GetHashCode(IPEndPoint obj) => obj.Address.GetHashCode() ^ obj.Port;
        }
    }
}
