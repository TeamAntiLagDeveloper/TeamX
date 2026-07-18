namespace TeamX.Shared.DTOs;

public class ErembyWebhookRequest
{
    public string PaymentStatus { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Guid PlanId { get; set; }

    public string TransactionId { get; set; } = string.Empty;
}