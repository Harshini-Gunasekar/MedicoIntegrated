using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Booking.Services
{
    public class DeviceNetworkDetails
    {
        public string DeviceName { get; set; } = Environment.MachineName;
        public string WifiName { get; set; } = "Wi-Fi";
        public string LocalIp { get; set; } = "";
        public string Signal { get; set; } = "";
        public string Band { get; set; } = "";
        public bool IsConnected { get; set; } = true;
    }

    public class NetworkInfoService
    {
        private DeviceNetworkDetails? _cachedDetails;
        private DateTime _lastFetchTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(15);
        private readonly object _lock = new();

        public async Task<DeviceNetworkDetails> GetDeviceNetworkDetailsAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _cachedDetails != null && (DateTime.UtcNow - _lastFetchTime) < CacheDuration)
            {
                return _cachedDetails;
            }

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!forceRefresh && _cachedDetails != null && (DateTime.UtcNow - _lastFetchTime) < CacheDuration)
                    {
                        return _cachedDetails;
                    }

                    var details = new DeviceNetworkDetails
                    {
                        DeviceName = Environment.MachineName
                    };

                    try
                    {
                        // Get Local IPv4 Address
                        var host = Dns.GetHostEntry(Dns.GetHostName());
                        var localIp = host.AddressList.FirstOrDefault(ip => 
                            ip.AddressFamily == AddressFamily.InterNetwork && 
                            !IPAddress.IsLoopback(ip) && 
                            !ip.ToString().StartsWith("169.254"))?.ToString();

                        details.LocalIp = localIp ?? "";
                    }
                    catch { }

                    // Parse Wi-Fi interface using netsh on Windows
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "wlan show interfaces",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using var proc = Process.Start(psi);
                        if (proc != null)
                        {
                            string output = proc.StandardOutput.ReadToEnd();
                            proc.WaitForExit(1500);

                            if (!string.IsNullOrWhiteSpace(output))
                            {
                                var ssidMatch = Regex.Match(output, @"^\s*SSID\s*:\s*(.+)$", RegexOptions.Multiline);
                                if (ssidMatch.Success && !string.IsNullOrWhiteSpace(ssidMatch.Groups[1].Value))
                                {
                                    details.WifiName = ssidMatch.Groups[1].Value.Trim();
                                }

                                var signalMatch = Regex.Match(output, @"^\s*Signal\s*:\s*(.+)$", RegexOptions.Multiline);
                                if (signalMatch.Success)
                                {
                                    details.Signal = signalMatch.Groups[1].Value.Trim();
                                }

                                var bandMatch = Regex.Match(output, @"^\s*Band\s*:\s*(.+)$", RegexOptions.Multiline);
                                if (bandMatch.Success)
                                {
                                    details.Band = bandMatch.Groups[1].Value.Trim();
                                }

                                var stateMatch = Regex.Match(output, @"^\s*State\s*:\s*(.+)$", RegexOptions.Multiline);
                                if (stateMatch.Success && stateMatch.Groups[1].Value.Contains("disconnected", StringComparison.OrdinalIgnoreCase))
                                {
                                    details.IsConnected = false;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // If netsh fails, fallback to NetworkInterface inspection
                    }

                    // If Wi-Fi SSID wasn't found (e.g. Ethernet / wired LAN connection)
                    if (string.IsNullOrWhiteSpace(details.WifiName) || details.WifiName == "Wi-Fi")
                    {
                        try
                        {
                            var activeNic = NetworkInterface.GetAllNetworkInterfaces()
                                .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up && 
                                                       nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                                       !nic.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase));

                            if (activeNic != null)
                            {
                                if (activeNic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                                {
                                    details.WifiName = activeNic.Name ?? "Wi-Fi";
                                }
                                else if (activeNic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                                {
                                    details.WifiName = "Ethernet (LAN)";
                                }
                            }
                        }
                        catch { }
                    }

                    _cachedDetails = details;
                    _lastFetchTime = DateTime.UtcNow;
                    return details;
                }
            });
        }
    }
}
