namespace TeamX.Shared.DTOs;

public class SecureActivateRequest
{
    public string LicenseKey { get; set; } = string.Empty;
    public string HardwareFingerprint { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public string ExecutableHash { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string? ComputerName { get; set; }
    public string? WindowsVersion { get; set; }
}