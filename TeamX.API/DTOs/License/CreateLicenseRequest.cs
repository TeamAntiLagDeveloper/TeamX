namespace TeamX.API.DTOs.License;
using TeamX.Shared.DTOs;

public class CreateLicenseRequest
{
    public Guid CustomerId { get; set; }

    public Guid ProductId { get; set; }

    public Guid PlanId { get; set; }
}