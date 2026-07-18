using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public static class LicenseStorage
{
    private static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TeamX");

    private static readonly string LicenseFile = Path.Combine(AppDataPath, "license.dat");
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("TeamX-License-Entropy-2026-v2");

    public static void Save(ActivateResponse response, string hardwareFingerprint)
    {
        Directory.CreateDirectory(AppDataPath);

        var data = new StoredLicense
        {
            Token = response.Token,
            HardwareFingerprint = hardwareFingerprint,
            ActivatedAt = DateTime.UtcNow,
            ExpiresAt = response.ExpiresAt
        };

        string json = JsonSerializer.Serialize(data);
        byte[] encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(json),
            Entropy,
            DataProtectionScope.CurrentUser);

        File.WriteAllBytes(LicenseFile, encrypted);
    }

    public static StoredLicense? Load()
    {
        if (!File.Exists(LicenseFile)) return null;

        try
        {
            byte[] encrypted = File.ReadAllBytes(LicenseFile);
            byte[] decrypted = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
            string json = Encoding.UTF8.GetString(decrypted);
            return JsonSerializer.Deserialize<StoredLicense>(json);
        }
        catch
        {
            return null;
        }
    }

    public static void Clear()
    {
        if (File.Exists(LicenseFile))
            File.Delete(LicenseFile);
    }
}

public class StoredLicense
{
    public string Token { get; set; } = string.Empty;
    public string HardwareFingerprint { get; set; } = string.Empty;
    public DateTime ActivatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}