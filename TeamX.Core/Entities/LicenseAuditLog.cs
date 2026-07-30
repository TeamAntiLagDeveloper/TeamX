namespace TeamX.Core.Entities;

public class LicenseAuditLog
{
    public Guid Id { get; set; }

    public int LicenseId { get; set; }
    public License? License { get; set; }

    /// <summary>
    /// Activate | Heartbeat | Abuse | Revoke | Validate | Suspend
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    public string? HardwareId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class LicenseAuditEventTypes
{
    public const string Activate = "Activate";
    public const string Heartbeat = "Heartbeat";
    public const string Abuse = "Abuse";
    public const string Revoke = "Revoke";
    public const string Validate = "Validate";
    public const string Suspend = "Suspend";
}