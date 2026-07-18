using TeamX.Core.Entities;
using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface ILicenseService
{
    Task<License> CreateLicenseAsync(CreateLicenseRequest request);

    Task<License> CreatePendingLicenseAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        int maxDevices,
        DateTime expiresAt);

    Task<License?> GetByKeyAsync(string key);

    Task<bool> ValidateAsync(string key);

    Task<License?> GetByKeyWithDevicesAsync(string key);
}