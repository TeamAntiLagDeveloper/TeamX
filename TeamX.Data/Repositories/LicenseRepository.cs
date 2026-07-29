using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.Data.Repositories;

/// <summary>
/// Repositório de acesso a dados de licenças.
/// </summary>
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
        return await _context.Licenses
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Product)
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _context.Licenses
            .AsNoTracking()
            .AnyAsync(x => x.Key == key, cancellationToken);
    }

    public async Task AddAsync(
        License license,
        CancellationToken cancellationToken = default)
    {
        await _context.Licenses.AddAsync(license, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<LicenseDevice?> GetDeviceAsync(
        string licenseKey,
        string hardwareId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LicenseDevices
            .AsNoTracking()
            .Include(x => x.License)
            .FirstOrDefaultAsync(x =>
                x.License.Key == licenseKey &&
                x.HardwareId == hardwareId,
                cancellationToken);
    }
}