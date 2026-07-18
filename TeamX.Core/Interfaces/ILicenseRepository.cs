using TeamX.Core.Entities;

namespace TeamX.Core.Interfaces;

public interface ILicenseRepository
{
    Task<License?> GetByKeyAsync(string key);

    Task<bool> ExistsAsync(string key);

    Task AddAsync(License license);

    Task SaveChangesAsync();

    Task<LicenseDevice?> GetDeviceAsync(
        string licenseKey,
        string hardwareId);
}