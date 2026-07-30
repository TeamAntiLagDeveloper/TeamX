using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamX.Core.Entities;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class AbuseDetectionOptions
{
    public const string SectionName = "AbuseDetection";

    public int MaxDistinctHardwareIn24h { get; set; } = 8;
    public int MaxDistinctIpIn24h { get; set; } = 15;
    public int DeviceMargin { get; set; } = 2;
    public int HardwareMultiplier { get; set; } = 3;
}

public class AbuseDetectionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AbuseDetectionService> _logger;
    private readonly AbuseDetectionOptions _options;

    public AbuseDetectionService(
        ApplicationDbContext context,
        ILogger<AbuseDetectionService> logger,
        IOptions<AbuseDetectionOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
    }

    public async Task EvaluateLicenseAsync(int licenseId, CancellationToken ct = default)
    {
        var license = await _context.Licenses
            .AsNoTracking()
            .Where(x => x.Id == licenseId)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.MaxDevices,
                ActiveDevices = x.Devices.Count(d => d.IsActive)
            })
            .FirstOrDefaultAsync(ct);

        if (license is null || license.Status is "Revoked" or "Suspended")
            return;

        var since = DateTime.UtcNow.AddHours(-24);

        var stats = await _context.LicenseAuditLogs
            .Where(x => x.LicenseId == licenseId && x.CreatedAt >= since)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                DistinctHardware = g
                    .Where(x => x.HardwareId != null && x.HardwareId != "")
                    .Select(x => x.HardwareId!)
                    .Distinct()
                    .Count(),
                DistinctIp = g
                    .Where(x => x.IpAddress != null && x.IpAddress != "")
                    .Select(x => x.IpAddress!)
                    .Distinct()
                    .Count()
            })
            .FirstOrDefaultAsync(ct);

        var distinctHw = stats?.DistinctHardware ?? 0;
        var distinctIp = stats?.DistinctIp ?? 0;
        var activeDevices = license.ActiveDevices;
        var maxAllowed = Math.Max(license.MaxDevices, 1);

        var isAbuse =
            distinctHw > Math.Max(_options.MaxDistinctHardwareIn24h, maxAllowed * _options.HardwareMultiplier) ||
            distinctIp > _options.MaxDistinctIpIn24h ||
            activeDevices > maxAllowed + _options.DeviceMargin;

        if (!isAbuse)
            return;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            var licenseToUpdate = await _context.Licenses
                .FirstOrDefaultAsync(x => x.Id == licenseId, ct);

            if (licenseToUpdate is null || licenseToUpdate.Status is "Revoked" or "Suspended")
            {
                await transaction.RollbackAsync(ct);
                return;
            }

            licenseToUpdate.Status = "Suspended";
            licenseToUpdate.UpdatedAt = DateTime.UtcNow;

            _context.LicenseAuditLogs.Add(new LicenseAuditLog
            {
                Id = Guid.NewGuid(),
                LicenseId = licenseId,
                EventType = "Abuse",
                Details =
                    $"HW24h={distinctHw}; IP24h={distinctIp}; ActiveDevices={activeDevices}; MaxDevices={maxAllowed}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogWarning(
                "Licença {LicenseId} suspensa por abuso. HW={Hw} IP={Ip} Devices={Devices}/{Max}",
                licenseId, distinctHw, distinctIp, activeDevices, maxAllowed);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Falha ao suspender licença {LicenseId} por abuso", licenseId);
            throw;
        }
    }

    public async Task LogAsync(
        int licenseId,
        string eventType,
        string? hardwareId,
        string? ip,
        string? details = null,
        CancellationToken ct = default)
    {
        try
        {
            _context.LicenseAuditLogs.Add(new LicenseAuditLog
            {
                Id = Guid.NewGuid(),
                LicenseId = licenseId,
                EventType = eventType,
                HardwareId = hardwareId,
                IpAddress = ip,
                Details = details,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Auditoria nunca deve derrubar activate/heartbeat
            _logger.LogWarning(
                ex,
                "Falha ao gravar audit log. LicenseId={LicenseId} Event={Event}",
                licenseId,
                eventType);
        }
    }
}