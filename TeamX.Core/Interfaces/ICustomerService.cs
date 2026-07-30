using TeamX.Core.Entities;

namespace TeamX.Core.Interfaces;

public interface ICustomerService
{
    Task<Customer> GetOrCreateAsync(
        string email,
        CancellationToken cancellationToken = default);
}