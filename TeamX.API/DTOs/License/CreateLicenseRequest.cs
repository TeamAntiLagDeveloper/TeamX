using System.ComponentModel.DataAnnotations;

namespace TeamX.API.DTOs.License;

/// <summary>
/// Request para criação de uma nova licença.
/// </summary>
public record CreateLicenseRequest
{
    /// <summary>
    /// Identificador do cliente.
    /// </summary>
    [Required(ErrorMessage = "CustomerId é obrigatório")]
    public required Guid CustomerId { get; init; }

    /// <summary>
    /// Identificador do produto.
    /// </summary>
    [Required(ErrorMessage = "ProductId é obrigatório")]
    public required Guid ProductId { get; init; }

    /// <summary>
    /// Identificador do plano.
    /// </summary>
    [Required(ErrorMessage = "PlanId é obrigatório")]
    public required Guid PlanId { get; init; }
}