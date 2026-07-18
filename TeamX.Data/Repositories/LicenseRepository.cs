using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.Data.Repositories;

public class LicenseRepository : ILicenseRepository
{
    private readonly ApplicationDbContext _context;

    public LicenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<License?> GetByKeyAsync(string key)
    {
        return await _context.Licenses
            .Include(x => x.Customer)
            .Include(x => x.Product)
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key == key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _context.Licenses
            .AnyAsync(x => x.Key == key);
    }

    public async Task AddAsync(License license)
    {
        await _context.Licenses.AddAsync(license);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<LicenseDevice?> GetDeviceAsync(
    string licenseKey,
    string hardwareId)
    {
        return await _context.LicenseDevices
            .Include(x => x.License)
            .FirstOrDefaultAsync(x =>
                x.License.Key == licenseKey &&
                x.HardwareId == hardwareId);
    }
}