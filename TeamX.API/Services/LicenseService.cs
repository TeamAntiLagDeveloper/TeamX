using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Security.Licensing;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class LicenseService : ILicenseService
{
    private const int MaxKeyGenerationAttempts = 5;

    private readonly ApplicationDbContext _context;
    private readonly ILicenseKeyGenerator _keyGenerator;
    private readonly ILogger<LicenseService> _logger;

    public LicenseService(
        ApplicationDbContext context,
        ILicenseKeyGenerator keyGenerator,
        ILogger<LicenseService> logger)
    {
        _context = context;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────
    // Lookups auxiliares
    // ─────────────────────────────────────────────────────────────

    public async Task<Product?> GetTeamXProductAsync(
        string variantId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(variantId))
            return null;

        var plan = await _context.Plans
            .AsNoTracking()
            .Where(x => x.Code == variantId && x.IsActive)
            .Select(x => new { x.ProductId })
            .FirstOrDefaultAsync(ct);

        if (plan is null)
            return null;

        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == plan.ProductId && x.IsActive, ct);
    }

    public async Task<Plan?> GetTeamXPlanAsync(
        string variantId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(variantId))
            return null;

        return await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == variantId && x.IsActive, ct);
    }

    public async Task<License?> GetByKeyAsync(
        string key,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var normalizedKey = NormalizeKey(key);

        return await _context.Licenses
            .Include(x => x.Customer)
            .Include(x => x.Product)
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key == normalizedKey, ct);
    }

    public async Task<License?> GetByKeyWithDevicesAsync(
        string key,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var normalizedKey = NormalizeKey(key);

        return await _context.Licenses
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key == normalizedKey, ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Validação (leve — não carrega o grafo inteiro)
    // ─────────────────────────────────────────────────────────────

    public async Task<bool> ValidateAsync(
        string key,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var normalizedKey = NormalizeKey(key);
        var now = DateTime.UtcNow;

        return await _context.Licenses
            .AsNoTracking()
            .AnyAsync(x =>
                x.Key == normalizedKey &&
                x.Status == "Active" &&
                x.ExpiresAt > now,
                ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Criação
    // ─────────────────────────────────────────────────────────────

    public async Task<License> CreateLicenseAsync(
        CreateLicenseRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PlanId == request.PlanId, ct)
            ?? throw new InvalidOperationException($"Plano {request.PlanId} não encontrado.");

        var expiresAt = plan.IsLifetime
            ? DateTime.UtcNow.AddYears(100)   // evita DateTime.MaxValue (problemas em alguns bancos)
            : DateTime.UtcNow.AddDays(plan.DurationDays);

        var license = new License
        {
            Key = await GenerateUniqueKeyAsync(ct),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            PlanId = request.PlanId,
            MaxDevices = plan.MaxDevices
        };

        _context.Licenses.Add(license);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Licença criada. Key={Key} PlanId={PlanId} CustomerId={CustomerId}",
            MaskKey(license.Key),
            license.PlanId,
            license.CustomerId);

        return license;
    }

    public async Task<License> CreatePendingLicenseAsync(
        Guid customerId,
        Guid productId,
        Guid planId,
        int durationDays,
        DateTime expiresAt,
        CancellationToken ct = default)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PlanId == planId, ct)
            ?? throw new InvalidOperationException($"Plano {planId} não encontrado.");

        var license = new License
        {
            Key = await GenerateUniqueKeyAsync(ct),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CustomerId = customerId,
            ProductId = productId,
            PlanId = planId,
            MaxDevices = plan.MaxDevices
        };

        _context.Licenses.Add(license);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Licença pendente criada. Key={Key} PlanId={PlanId} CustomerId={CustomerId}",
            MaskKey(license.Key),
            license.PlanId,
            license.CustomerId);

        return license;
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers privados
    // ─────────────────────────────────────────────────────────────

    private async Task<string> GenerateUniqueKeyAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxKeyGenerationAttempts; attempt++)
        {
            var key = NormalizeKey(_keyGenerator.Generate());

            var exists = await _context.Licenses
                .AsNoTracking()
                .AnyAsync(x => x.Key == key, ct);

            if (!exists)
                return key;
        }

        throw new InvalidOperationException(
            "Não foi possível gerar uma chave de licença única após várias tentativas.");
    }

    private static string NormalizeKey(string key)
        => key.Trim().ToUpperInvariant();

    private static string MaskKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8)
            return "***";

        return $"{key[..4]}...{key[^4..]}";
    }
}