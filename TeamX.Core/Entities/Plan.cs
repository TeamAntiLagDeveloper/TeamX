namespace TeamX.Core.Entities;

public class Plan
{
    public Guid PlanId { get; set; }

    public ICollection<License> Licenses { get; set; } = new List<License>();


    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;


    public string Name { get; set; } = string.Empty;


    // Código usado pela Eremby/Webhook
    public string Code { get; set; } = string.Empty;


    public int DurationDays { get; set; }


    public decimal Price { get; set; }


    public int MaxDevices { get; set; }


    public bool IsLifetime { get; set; }


    public bool IsActive { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}