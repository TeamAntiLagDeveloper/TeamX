using TeamX.Core.Entities;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável pelas operações de pedidos (Orders).
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Cria um novo pedido.
    /// </summary>
    Task<Order> CreateAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        string email,
        string transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Associa uma licença a um pedido existente.
    /// </summary>
    Task UpdateLicenseAsync(
        Guid orderId,
        int licenseId,
        CancellationToken cancellationToken = default);
}