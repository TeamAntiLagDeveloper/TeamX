namespace TeamX.Core.Entities;

public class Customer
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Discord { get; set; }

    public string Country { get; set; } = "Brasil";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<License> Licenses { get; set; } = new List<License>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}