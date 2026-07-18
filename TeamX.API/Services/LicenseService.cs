using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Security.Licensing;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class LicenseService : ILicenseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILicenseKeyGenerator _keyGenerator;

    public LicenseService(
        ApplicationDbContext context,
        ILicenseKeyGenerator keyGenerator)
    {
        _context = context;
        _keyGenerator = keyGenerator;
    }


    public async Task<License?> GetByKeyWithDevicesAsync(string key)
    {
        return await _context.Licenses
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key == key);
    }

    public async Task<License> CreateLicenseAsync(CreateLicenseRequest request)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(x => x.PlanId == request.PlanId);


        if (plan == null)
            throw new Exception("Plano não encontrado");

        DateTime expiresAt;


        if (plan.IsLifetime)
        {
            expiresAt = DateTime.MaxValue;
        }
        else
        {
            expiresAt = DateTime.UtcNow
                .AddDays(plan.DurationDays);
        }

        var key = _keyGenerator.Generate();

        while (await _context.Licenses.AnyAsync(x => x.Key == key))
        {
            key = _keyGenerator.Generate();
        }

        var license = new License
        {
            Key = key,

            Status = "Active",

            CreatedAt = DateTime.UtcNow,

            ExpiresAt = expiresAt,

            CustomerId = request.CustomerId,

            ProductId = request.ProductId,

            PlanId = request.PlanId,

            MaxDevices = plan.MaxDevices
        };


        _context.Licenses.Add(license);


        await _context.SaveChangesAsync();


        return license;
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


    public async Task<bool> ValidateAsync(string key)
    {
        var license = await GetByKeyAsync(key);

        if (license == null)
            return false;


        return license.Status == "Active"
            && license.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<License> CreatePendingLicenseAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        int durationDays,
        DateTime expiresAt)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(x => x.PlanId == planId);

        if (plan == null)
            throw new Exception("Plano não encontrado");


        var license = new License
        {
            Key = _keyGenerator.Generate(),

            Status = "Pending",

            CreatedAt = DateTime.UtcNow,

            ExpiresAt = expiresAt,

            CustomerId = customerId,

            ProductId = productId,

            PlanId = planId,

            MaxDevices = plan.MaxDevices
        };


        _context.Licenses.Add(license);

        await _context.SaveChangesAsync();

        return license;
    }
}