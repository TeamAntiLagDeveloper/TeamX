namespace TeamX.Core.Entities;

public class License
{
    public int Id { get; set; }

    /// <summary>
    /// Chave de ativação (UPPER + trim).
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Pending | Active | Suspended | Expired | Revoked
    /// </summary>
    public string Status { get; set; } = LicenseStatuses.Pending;

    public int MaxDevices { get; set; } = 1;

    /// <summary>
    /// True após a primeira ativação bem-sucedida.
    /// </summary>
    public bool IsActivated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public ICollection<LicenseDevice> Devices { get; set; } = new List<LicenseDevice>();
}

public static class LicenseStatuses
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Expired = "Expired";
    public const string Revoked = "Revoked";
}