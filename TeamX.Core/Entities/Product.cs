using TeamX.Core.Enums;

namespace TeamX.Core.Entities;

public class Product
{
    public Guid Id { get; set; }
    public ICollection<License> Licenses { get; set; } = new List<License>();
    public string Name { get; set; } = string.Empty;
    // Código usado pelo webhook/Eremby
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Plan> Plans { get; set; } = new List<Plan>();
}