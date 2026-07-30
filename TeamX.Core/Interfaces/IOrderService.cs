using TeamX.Core.Entities;

namespace TeamX.Core.Interfaces;

public interface IOrderService
{
    Task<Order> CreateAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        string email,
        string transactionId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateLicenseAsync(
        Guid orderId,
        int licenseId,
        CancellationToken cancellationToken = default);
}