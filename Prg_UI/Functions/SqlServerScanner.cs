using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            // ➊ UDP network scan and Subnet TCP scan run in parallel for maximum speed
            var udpTask = Task.Run(() =>
            {
                foreach (var s in ScanUdpNetwork(udpTimeoutMs)) servers.TryAdd(s, 0);
            });

            var subnetTcpTask = Task.Run(() =>
            {
                foreach (var s in ScanLocalSubnetForSql()) servers.TryAdd(s, 0);
            });

            // ➋ Local registry is instant – run on current thread while network scans run
            RegistryView[] views = Environment.Is64BitOperatingSystem
                ? new[] { RegistryView.Registry64, RegistryView.Registry32 }
                : new[] { RegistryView.Registry32 };

            foreach (var view in views)
                foreach (var s in GetSqlInstancesFromRegistry(view))
                    servers.TryAdd(s, 0);

            Task.WaitAll(udpTask, subnetTcpTask);

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
                    catch (SocketException) { break; }
                    catch { /* ignore */ }
                }
            }

            foreach (var s in results.Distinct(StringComparer.OrdinalIgnoreCase))
                yield return s;
        }

        private static IEnumerable<string> ParseServerInfoList(byte[] payload, IPAddress ip)
        {
            if (payload.Length <= 3) yield break;

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
                    server = null;
                }
            }
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
        //        DYNAMIC SUBNET TCP PORT CHECK (Replaces Hardcoded Hostnames)
        //        Catches servers blocking UDP 1434 by sweeping the local /24 subnet.
        // ---------------------------------------------------------------------------
        private static IEnumerable<string> ScanLocalSubnetForSql(int timeoutMs = 300)
        {
            var foundServers = new ConcurrentBag<string>();
            var ipAddressesToScan = new List<IPAddress>();

            // Find local IPv4 /24 subnets
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var nic in interfaces)
            {
                var props = nic.GetIPProperties();
                foreach (var unicast in props.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork && unicast.IPv4Mask != null)
                    {
                        byte[] ipBytes = unicast.Address.GetAddressBytes();
                        byte[] maskBytes = unicast.IPv4Mask.GetAddressBytes();

                        // Optimize for standard Class C (/24) subnets
                        if (maskBytes[0] == 255 && maskBytes[1] == 255 && maskBytes[2] == 255)
                        {
                            for (int i = 1; i < 255; i++)
                            {
                                ipAddressesToScan.Add(new IPAddress(new byte[] { ipBytes[0], ipBytes[1], ipBytes[2], (byte)i }));
                            }
                        }
                    }
                }
            }

            // Parallel sweep on port 1433
            Parallel.ForEach(ipAddressesToScan, ip =>
            {
                try
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync(ip.ToString(), 1433);

                    if (connectTask.Wait(timeoutMs) && client.Connected)
                    {
                        string discoveredName = ip.ToString();

                        // Attempt reverse DNS to get actual hostname (e.g., 'DB2')
                        try
                        {
                            var entry = Dns.GetHostEntry(ip);
                            if (!string.IsNullOrEmpty(entry.HostName))
                            {
                                discoveredName = entry.HostName.Split('.')[0].ToUpper();
                            }
                        }
                        catch { /* Fallback to IP if reverse DNS fails */ }

                        foundServers.Add(discoveredName);
                    }
                }
                catch { /* Ignore dead IP addresses */ }
            });

            return foundServers.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private class IPEndPointComparer : IEqualityComparer<IPEndPoint>
        {
            public bool Equals(IPEndPoint x, IPEndPoint y) => x.Address.Equals(y.Address) && x.Port == y.Port;
            public int GetHashCode(IPEndPoint obj) => obj.Address.GetHashCode() ^ obj.Port;
        }
    }
}