using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace TeamX.App.Services;

public static class HardwareService
{
    private static readonly object FingerprintLock = new();
    private static string? _cachedFingerprint;
    private static string? _cachedExeHash;

    /// <summary>
    /// Fingerprint estável da máquina (cacheado em memória).
    /// </summary>
    public static string GetStrongFingerprint()
    {
        if (_cachedFingerprint is not null)
            return _cachedFingerprint;

        lock (FingerprintLock)
        {
            if (_cachedFingerprint is not null)
                return _cachedFingerprint;

            try
            {
                // Preferir componentes estáveis. MAC é frágil (VPN, dongle, Wi‑Fi on/off).
                var components = new[]
                {
                    GetProcessorId(),
                    GetMotherboardSerial(),
                    GetBiosSerial(),
                    GetSystemDriveSerial(),
                    GetFirstFixedDiskSerial()
                };

                var combined = string.Join('|', components.Select(Normalize));
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
                _cachedFingerprint = Convert.ToHexString(hash);
            }
            catch
            {
                // Fallback determinístico (fraco, mas estável entre chamadas na mesma sessão)
                var raw = $"{Environment.MachineName}|{Environment.OSVersion}|{Environment.ProcessorCount}";
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
                _cachedFingerprint = "FALLBACK_" + Convert.ToHexString(hash)[..16];
            }

            return _cachedFingerprint;
        }
    }

    public static string GetExecutableHash()
    {
        if (_cachedExeHash is not null)
            return _cachedExeHash;

        try
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
                return _cachedExeHash = "UNKNOWN_HASH";

            using var stream = File.OpenRead(exePath);
            var hash = SHA256.HashData(stream);
            _cachedExeHash = Convert.ToHexString(hash);
        }
        catch
        {
            _cachedExeHash = "UNKNOWN_HASH";
        }

        return _cachedExeHash;
    }

    /// <summary>
    /// Força recálculo (ex.: após troca de hardware detectada).
    /// </summary>
    public static void InvalidateCache()
    {
        lock (FingerprintLock)
        {
            _cachedFingerprint = null;
            _cachedExeHash = null;
        }
    }

    // ─── Componentes ─────────────────────────────────────────────

    private static string GetProcessorId()
    {
        return QueryWmiFirst(
            "SELECT ProcessorId FROM Win32_Processor",
            "ProcessorId",
            "UNKNOWN_CPU");
    }

    private static string GetMotherboardSerial()
    {
        return QueryWmiFirst(
            "SELECT SerialNumber FROM Win32_BaseBoard",
            "SerialNumber",
            "UNKNOWN_MB");
    }

    private static string GetBiosSerial()
    {
        return QueryWmiFirst(
            "SELECT SerialNumber FROM Win32_BIOS",
            "SerialNumber",
            "UNKNOWN_BIOS");
    }

    private static string GetSystemDriveSerial()
    {
        // Volume serial do drive do sistema (mais estável que só "C:")
        try
        {
            var systemRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            var deviceId = systemRoot.TrimEnd('\\');

            return QueryWmiFirst(
                $"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '{deviceId}'",
                "VolumeSerialNumber",
                "UNKNOWN_VOL");
        }
        catch
        {
            return "UNKNOWN_VOL";
        }
    }

    private static string GetFirstFixedDiskSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT SerialNumber, MediaType, Index FROM Win32_DiskDrive");

            // Ordena por Index para resultado determinístico
            var serials = new List<(int Index, string Serial)>();

            foreach (ManagementObject obj in searcher.Get())
            {
                using (obj)
                {
                    var serial = obj["SerialNumber"]?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(serial) || IsPlaceholderSerial(serial))
                        continue;

                    var index = 0;
                    try { index = Convert.ToInt32(obj["Index"] ?? 0); } catch { /* ignore */ }

                    serials.Add((index, serial));
                }
            }

            if (serials.Count == 0)
                return "UNKNOWN_DISK";

            return serials.OrderBy(x => x.Index).First().Serial;
        }
        catch
        {
            return "UNKNOWN_DISK";
        }
    }

    /// <summary>
    /// MAC opcional — NÃO entra no fingerprint padrão (muda com frequência).
    /// Útil só para telemetria / logs.
    /// </summary>
    public static string? TryGetPrimaryMacAddress()
    {
        try
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType is not NetworkInterfaceType.Loopback
                        and not NetworkInterfaceType.Tunnel)
                .OrderByDescending(n => n.Speed)
                .ThenBy(n => n.Id, StringComparer.Ordinal)
                .FirstOrDefault();

            var mac = nic?.GetPhysicalAddress().ToString();
            if (string.IsNullOrWhiteSpace(mac) || mac is "000000000000")
                return null;

            return mac;
        }
        catch
        {
            return null;
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static string QueryWmiFirst(string query, string property, string fallback)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject obj in searcher.Get())
            {
                using (obj)
                {
                    var value = obj[property]?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(value) && !IsPlaceholderSerial(value))
                        return value;
                }
            }
        }
        catch
        {
            // WMI pode falhar em VMs, containers, permissões
        }

        return fallback;
    }

    private static bool IsPlaceholderSerial(string value)
    {
        var v = value.Trim();
        if (v.Length == 0)
            return true;

        // Seriais genéricos comuns em OEM / VMs
        return v.Equals("To Be Filled By O.E.M.", StringComparison.OrdinalIgnoreCase)
            || v.Equals("Default string", StringComparison.OrdinalIgnoreCase)
            || v.Equals("None", StringComparison.OrdinalIgnoreCase)
            || v.Equals("0", StringComparison.Ordinal)
            || v.All(c => c is '0' or ' ' or '-');
    }

    private static string Normalize(string value)
        => string.IsNullOrWhiteSpace(value)
            ? "UNKNOWN"
            : value.Trim().ToUpperInvariant();
}