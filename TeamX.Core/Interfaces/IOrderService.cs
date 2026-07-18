using TeamX.Core.Entities;

namespace TeamX.Core.Interfaces;

public interface IOrderService
{
    Task<Order> CreateAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        string email,
        string transactionId);


    Task UpdateLicenseAsync(
        Guid orderId,
        int licenseId);
}