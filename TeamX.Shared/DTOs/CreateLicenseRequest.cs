namespace TeamX.Shared.DTOs;

/// <summary>
/// Request para criação de uma nova licença.
/// </summary>
public class CreateLicenseRequest
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public Guid PlanId { get; set; }
}