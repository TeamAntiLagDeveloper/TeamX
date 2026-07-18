using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace TeamX.App.Services;

public static class HardwareService
{
    public static string GetStrongFingerprint()
    {
        try
        {
            var components = new List<string>
            {
                GetProcessorId(),
                GetDiskSerial(),
                GetMotherboardSerial(),
                GetMacAddress(),
                Environment.MachineName,
                Environment.UserName
            };

            string combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));

            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hash);
        }
        catch
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
        }
    }

    private static string GetProcessorId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["ProcessorId"]?.ToString() ?? "UNKNOWN_CPU";
            }
        }
        catch { }
        return "UNKNOWN_CPU";   // ← Adicione esta linha
    }

    private static string GetDiskSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString() ?? "UNKNOWN_DISK";
            }
        }
        catch { }
        return "UNKNOWN_DISK";   // ← Adicione esta linha
    }

    private static string GetMotherboardSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
                return obj["SerialNumber"]?.ToString() ?? "UNKNOWN_MB";
        }
        catch { }
        return "UNKNOWN_MB";
    }

    private static string GetMacAddress()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)?
                .GetPhysicalAddress().ToString() ?? "UNKNOWN_MAC";
        }
        catch { }
        return "UNKNOWN_MAC";
    }

    public static string GetExecutableHash()
    {
        try
        {
            string exePath = Environment.ProcessPath!;
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(exePath);
            byte[] hash = sha256.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }
        catch { return "UNKNOWN_HASH"; }
    }
}