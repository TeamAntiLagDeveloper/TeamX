namespace TeamX.Core.Entities;

public class Order
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public int? LicenseId { get; set; }
    public License? License { get; set; }

    /// <summary>
    /// E-mail normalizado no momento da compra.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Pending | Paid | Refunded | Cancelled | Failed
    /// </summary>
    public string Status { get; set; } = OrderStatuses.Paid;

    /// <summary>
    /// ID da transação no gateway. Deve ser único.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Refunded = "Refunded";
    public const string Cancelled = "Cancelled";
    public const string Failed = "Failed";
}