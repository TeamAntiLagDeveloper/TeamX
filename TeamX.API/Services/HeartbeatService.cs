using Microsoft.EntityFrameworkCore;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class HeartbeatService : IHeartbeatService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<HeartbeatService> _logger;

    public HeartbeatService(
        ApplicationDbContext context,
        ITokenService tokenService,
        ILogger<HeartbeatService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<bool> RecordHeartbeatAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hardwareFingerprint))
            return false;

        var hw = hardwareFingerprint.Trim();
        var validation = await _tokenService.ValidateTokenAsync(token, hw, ct);

        if (!validation.IsValid)
        {
            _logger.LogDebug("Heartbeat rejeitado: token inválido. HW={HardwareId}", hw);
            return false;
        }

        var rowsAffected = await _context.LicenseDevices
            .Where(d =>
                d.HardwareId == hw &&
                d.IsActive &&
                d.LicenseId == validation.LicenseId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(d => d.LastSeen, DateTime.UtcNow),
                ct);

        if (rowsAffected == 0)
        {
            _logger.LogWarning(
                "Heartbeat: device não encontrado ou inativo. LicenseId={LicenseId} HW={HardwareId}",
                validation.LicenseId,
                hw);
            return false;
        }

        return true;
    }

    public async Task<bool> IsDeviceActiveAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hardwareFingerprint))
            return false;

        var hw = hardwareFingerprint.Trim();
        var validation = await _tokenService.ValidateTokenAsync(token, hw, ct);
        if (!validation.IsValid)
            return false;

        return await _context.LicenseDevices
            .AsNoTracking()
            .AnyAsync(d =>
                d.HardwareId == hw &&
                d.IsActive &&
                d.LicenseId == validation.LicenseId,
                ct);
    }
}