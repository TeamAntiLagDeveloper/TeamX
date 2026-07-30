namespace TeamX.Core.Entities;

public class Plan
{
    /// <summary>
    /// PK do plano (nome PlanId por compatibilidade com EF/código atual).
    /// </summary>
    public Guid PlanId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Código do variant no gateway (Eremby). Único.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Duração em dias. Ignorado se <see cref="IsLifetime"/> for true.
    /// </summary>
    public int DurationDays { get; set; }

    public decimal Price { get; set; }

    public int MaxDevices { get; set; } = 1;

    public bool IsLifetime { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<License> Licenses { get; set; } = new List<License>();
}