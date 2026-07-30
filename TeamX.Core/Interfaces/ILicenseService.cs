using TeamX.Core.Entities;
using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface ILicenseService
{
    Task<License> CreateLicenseAsync(
        CreateLicenseRequest request,
        CancellationToken cancellationToken = default);

    Task<License> CreatePendingLicenseAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        int maxDevices,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);

    Task<License?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<License?> GetByKeyWithDevicesAsync(
        string key,
        CancellationToken cancellationToken = default);
}