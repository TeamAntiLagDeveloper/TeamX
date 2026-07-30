namespace TeamX.Core.Entities;

public class LicenseDevice
{
    public Guid Id { get; set; }

    public int LicenseId { get; set; }
    public License License { get; set; } = null!;

    /// <summary>
    /// Fingerprint de hardware (hash SHA-256 hex).
    /// </summary>
    public string HardwareId { get; set; } = null!;

    public string? ComputerName { get; set; }
    public string? WindowsVersion { get; set; }
    public string? IpAddress { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}