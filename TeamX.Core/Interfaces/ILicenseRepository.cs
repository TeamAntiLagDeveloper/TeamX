using TeamX.Core.Entities;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Repositório de acesso a dados de licenças.
/// </summary>
public interface ILicenseRepository
{
    Task<License?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        License license,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<LicenseDevice?> GetDeviceAsync(
        string licenseKey,
        string hardwareId,
        CancellationToken cancellationToken = default);
}