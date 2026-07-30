using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.Data.Repositories;

public sealed class LicenseRepository : ILicenseRepository
{
    private readonly ApplicationDbContext _context;

    public LicenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<License?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var normalized = key.Trim().ToUpperInvariant();

        return await _context.Licenses
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Product)
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key == normalized, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var normalized = key.Trim().ToUpperInvariant();

        return await _context.Licenses
            .AsNoTracking()
            .AnyAsync(x => x.Key == normalized, cancellationToken);
    }

    public async Task AddAsync(
        License license,
        CancellationToken cancellationToken = default)
    {
        await _context.Licenses.AddAsync(license, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<LicenseDevice?> GetDeviceAsync(
        string licenseKey,
        string hardwareId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey) || string.IsNullOrWhiteSpace(hardwareId))
            return null;

        var key = licenseKey.Trim().ToUpperInvariant();
        var hw = hardwareId.Trim();

        return await _context.LicenseDevices
            .AsNoTracking()
            .Include(x => x.License)
            .FirstOrDefaultAsync(x =>
                x.License.Key == key &&
                x.HardwareId == hw,
                cancellationToken);
    }
}