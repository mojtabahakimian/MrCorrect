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

            // ➊ UDP network scan (broadcast)
            foreach (var s in ScanUdpNetwork(udpTimeoutMs)) servers.TryAdd(s, 0);

            // ➋ Local registry (64-bit and 32-bit views if available)
            RegistryView[] views = Environment.Is64BitOperatingSystem
                ? new[] { RegistryView.Registry64, RegistryView.Registry32 }
                : new[] { RegistryView.Registry32 };

            foreach (var view in views)
                foreach (var s in GetSqlInstancesFromRegistry(view))
                    servers.TryAdd(s, 0);

            // ➌ (Optional) Try pinging some common hostnames on LAN, NetBIOS style (uncomment if needed)
            //foreach (var s in ScanCommonHostnames())
            //    servers.TryAdd(s, 0);

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
                        if (resp.Length > 0)
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
        private static IEnumerable<string> ParseServerInfoList(byte[] payload, IPAddress ip)
        {
            var txt = Encoding.ASCII.GetString(payload);
            // Can contain multiple instances in one response (split by ; and look for ServerName/InstanceName pairs)
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
                }
            }
            // Some servers (default instance) might only return server name
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
        //        (OPTIONAL) FAST NETBIOS/LAN SWEEP FOR COMMON SQL HOSTNAMES
        // ---------------------------------------------------------------------------
        // This is only a bonus: it tries to ping common hostnames, e.g. MAIN, SQL, SERVER, etc.
        // Uncomment in GetAllSqlServerNames if you want this too.
        private static IEnumerable<string> ScanCommonHostnames()
        {
            // You can add more known/likely hostnames here
            string[] hostnames = { "MAIN", "SQL", "SERVER", "DBSERVER", "PC229" };
            var found = new ConcurrentBag<string>();
            Parallel.ForEach(hostnames, hn =>
            {
                try
                {
                    var ping = new Ping();
                    var reply = ping.Send(hn, 350);
                    if (reply.Status == IPStatus.Success)
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
