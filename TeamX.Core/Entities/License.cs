namespace TeamX.Core.Entities;

public class License
{
    public int Id { get; set; }

    // KEY gerada para o cliente
    public string Key { get; set; } = null!;

    // Pending, Active, Expired, Revoked
    public string Status { get; set; } = "Pending";

    // Quantos PCs podem usar
    public int MaxDevices { get; set; }

    // Se já foi ativada alguma vez
    public bool IsActivated { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }


    // Cliente dono
    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;


    // Produto comprado
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;


    // Plano
    public Guid PlanId { get; set; }

    public Plan Plan { get; set; } = null!;


    // Computadores vinculados
    public ICollection<LicenseDevice> Devices { get; set; }
        = new List<LicenseDevice>();
}