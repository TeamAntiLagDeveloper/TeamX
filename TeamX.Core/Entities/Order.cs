using System;

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


    public string CustomerEmail { get; set; } = string.Empty;


    public string Status { get; set; } = "Paid";


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string TransactionId { get; set; } = string.Empty;
}