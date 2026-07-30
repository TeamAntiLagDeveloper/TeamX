using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TeamX.Core.Constants;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public static class LicenseStorage
{
    private static readonly object FileLock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    private static readonly string AppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TeamX",
        "Secure");

    private static readonly string LicenseFile = Path.Combine(AppDataPath, "license.bin");

    public static void Save(ActivateResponse response, string hardwareFingerprint)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (string.IsNullOrWhiteSpace(response.Token))
            throw new InvalidOperationException("Token ausente na resposta de ativação.");

        if (string.IsNullOrWhiteSpace(hardwareFingerprint))
            throw new ArgumentException("HardwareFingerprint é obrigatório.", nameof(hardwareFingerprint));

        var data = new StoredLicense
        {
            Token = response.Token,
            HardwareFingerprint = hardwareFingerprint.Trim(),
            ActivatedAt = DateTime.UtcNow,
            ExpiresAt = response.ExpiresAt == default
                ? DateTime.UtcNow.AddDays(7)
                : response.ExpiresAt,
            ExecutableHash = HardwareService.GetExecutableHash(),
            AppVersion = SystemConstants.CurrentVersion ?? "1.0.0",
            Nonce = Guid.NewGuid().ToString("N"),
            LastSuccessfulOnlineValidation = DateTime.UtcNow
        };

        data.LocalSignature = ComputeLocalSignature(data);
        WriteEncrypted(data);
    }

    /// <summary>
    /// Atualiza só o timestamp de validação online bem-sucedida.
    /// </summary>
    public static void TouchOnlineValidation()
    {
        var stored = Load();
        if (stored is null)
            return;

        stored.LastSuccessfulOnlineValidation = DateTime.UtcNow;
        stored.LocalSignature = ComputeLocalSignature(stored);
        WriteEncrypted(stored);
    }

    public static StoredLicense? Load()
    {
        if (!File.Exists(LicenseFile))
            return null;

        try
        {
            byte[] encrypted;
            lock (FileLock)
            {
                encrypted = File.ReadAllBytes(LicenseFile);
            }

            var plain = ProtectedData.Unprotect(
                encrypted,
                GetMachineEntropy(),
                DataProtectionScope.CurrentUser);

            var data = JsonSerializer.Deserialize<StoredLicense>(plain, JsonOptions);
            if (data is null || string.IsNullOrWhiteSpace(data.Token))
            {
                Clear();
                return null;
            }

            // Integridade do blob (compara bytes do HMAC, não a string hex)
            var expected = ComputeLocalSignatureBytes(data);
            var provided = SafeFromHex(data.LocalSignature);

            if (provided is null ||
                !CryptographicOperations.FixedTimeEquals(expected, provided))
            {
                Clear();
                return null;
            }

            // Hardware mudou
            var currentHw = HardwareService.GetStrongFingerprint();
            if (!string.Equals(data.HardwareFingerprint, currentHw, StringComparison.OrdinalIgnoreCase))
            {
                Clear();
                return null;
            }

            // Executável modificado (anti-tamper básico)
            var currentExeHash = HardwareService.GetExecutableHash();
            if (!string.Equals(data.ExecutableHash, currentExeHash, StringComparison.OrdinalIgnoreCase))
            {
                Clear();
                return null;
            }

            return data;
        }
        catch
        {
            Clear();
            return null;
        }
    }

    public static void Clear()
    {
        try
        {
            lock (FileLock)
            {
                if (File.Exists(LicenseFile))
                    File.Delete(LicenseFile);

                var tmp = LicenseFile + ".tmp";
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
        }
        catch
        {
            // ignore
        }
    }

    public static bool Exists()
    {
        try
        {
            return File.Exists(LicenseFile);
        }
        catch
        {
            return false;
        }
    }

    // ─── Persistência ────────────────────────────────────────────

    private static void WriteEncrypted(StoredLicense data)
    {
        Directory.CreateDirectory(AppDataPath);

        var json = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);

        var encrypted = ProtectedData.Protect(
            json,
            GetMachineEntropy(),
            DataProtectionScope.CurrentUser);

        var temp = LicenseFile + ".tmp";

        lock (FileLock)
        {
            File.WriteAllBytes(temp, encrypted);
            File.Move(temp, LicenseFile, overwrite: true);

            try
            {
                File.SetAttributes(LicenseFile, FileAttributes.Hidden | FileAttributes.System);
            }
            catch
            {
                // atributos são cosméticos — não falhar por isso
            }
        }
    }

    // ─── Criptografia / assinatura local ─────────────────────────

    private static byte[] GetMachineEntropy()
    {
        var seed = HardwareService.GetStrongFingerprint() + "TeamX-2026-Secure-v3";
        return SHA256.HashData(Encoding.UTF8.GetBytes(seed));
    }

    private static string ComputeLocalSignature(StoredLicense data)
        => Convert.ToHexString(ComputeLocalSignatureBytes(data));

    private static byte[] ComputeLocalSignatureBytes(StoredLicense data)
    {
        var payload =
            $"{data.Token}|{data.HardwareFingerprint}|{data.ActivatedAt:O}|{data.ExpiresAt:O}|" +
            $"{data.ExecutableHash}|{data.AppVersion}|{data.Nonce}|{data.LastSuccessfulOnlineValidation:O}";

        return HMACSHA256.HashData(GetMachineEntropy(), Encoding.UTF8.GetBytes(payload));
    }

    private static byte[]? SafeFromHex(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return null;

        try
        {
            return Convert.FromHexString(hex.Trim());
        }
        catch
        {
            return null;
        }
    }
}

public sealed class StoredLicense
{
    public string Token { get; set; } = "";
    public string HardwareFingerprint { get; set; } = "";
    public DateTime ActivatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string ExecutableHash { get; set; } = "";
    public string AppVersion { get; set; } = "";
    public string Nonce { get; set; } = "";
    public string LocalSignature { get; set; } = "";

    /// <summary>
    /// Última vez que o servidor confirmou o token online (grace period offline).
    /// </summary>
    public DateTime LastSuccessfulOnlineValidation { get; set; }
}