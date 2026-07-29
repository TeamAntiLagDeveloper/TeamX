using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Security.Licensing;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class LicenseActivationService : ILicenseActivationService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly INonceService _nonceService;
    private readonly ILicenseKeyGenerator _keyGenerator;
    private readonly AbuseDetectionService _abuse;
    private readonly ILogger<LicenseActivationService> _logger;

    public LicenseActivationService(
        ApplicationDbContext context,
        ITokenService tokenService,
        INonceService nonceService,
        ILicenseKeyGenerator keyGenerator,
        AbuseDetectionService abuse,
        ILogger<LicenseActivationService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _nonceService = nonceService;
        _keyGenerator = keyGenerator;
        _abuse = abuse;
        _logger = logger;
    }

    public async Task<ActivateResponse> ActivateAsync(
        SecureActivateRequest request,
        ActivationContext context,
        CancellationToken ct = default)
    {
        // ─── Validações básicas ───────────────────────────────────────
        if (request is null || string.IsNullOrWhiteSpace(request.LicenseKey))
            return Fail("Dados inválidos");

        if (string.IsNullOrWhiteSpace(request.HardwareFingerprint))
            return Fail("HardwareFingerprint é obrigatório");

        var normalizedKey = request.LicenseKey.Trim().ToUpperInvariant();
        var hardwareId = request.HardwareFingerprint.Trim();

        if (!_keyGenerator.IsValidFormat(normalizedKey))
            return Fail("Formato de chave inválido");

        if (!await ValidateRequestSecurityAsync(request, ct))
            return Fail("Requisição inválida ou expirada");

        // ─── Carrega a licença ────────────────────────────────────────
        var license = await _context.Licenses
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key == normalizedKey, ct);

        if (license is null)
            return Fail("Licença inválida");

        if (license.Status != "Active")
            return Fail("Licença indisponível", status: license.Status);

        if (license.ExpiresAt < DateTime.UtcNow)
            return Fail("Licença expirada", status: "Expired");

        var maxDevices = ResolveMaxDevices(license);

        // ─── Ativação (com proteção contra race condition) ───────────
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            // Re-busca os devices dentro da transação para ter contagem atualizada
            var devices = await _context.LicenseDevices
                .Where(d => d.LicenseId == license.Id)
                .ToListAsync(ct);

            var existingDevice = devices.FirstOrDefault(d =>
                d.HardwareId.Equals(hardwareId, StringComparison.OrdinalIgnoreCase));

            var activeCount = devices.Count(d => d.IsActive);

            // Novo device e já está no limite
            if (existingDevice is null && activeCount >= maxDevices)
            {
                await transaction.RollbackAsync(ct);
                return Fail("Limite de dispositivos atingido", maxDevices: maxDevices);
            }

            var isNewDevice = existingDevice is null;
            var now = DateTime.UtcNow;

            if (existingDevice is not null)
            {
                // Reativação / refresh do device existente
                existingDevice.LastSeen = now;
                existingDevice.ComputerName = request.ComputerName ?? existingDevice.ComputerName;
                existingDevice.WindowsVersion = request.WindowsVersion ?? existingDevice.WindowsVersion;
                existingDevice.IpAddress = context.IpAddress ?? existingDevice.IpAddress;
                existingDevice.IsActive = true;
            }
            else
            {
                // Novo device
                var newDevice = new LicenseDevice
                {
                    Id = Guid.NewGuid(),
                    LicenseId = license.Id,
                    HardwareId = hardwareId,
                    ComputerName = request.ComputerName ?? "",
                    WindowsVersion = request.WindowsVersion ?? "",
                    IpAddress = context.IpAddress ?? "",
                    FirstSeen = now,
                    LastSeen = now,
                    IsActive = true
                };

                _context.LicenseDevices.Add(newDevice);
            }

            if (!license.IsActivated)
                license.IsActivated = true;

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // ─── Pós-ativação (fora da transação) ────────────────────
            var eventDetail = isNewDevice ? "New device" : "Existing device";

            await _abuse.LogAsync(
                license.Id,
                "Activate",
                hardwareId,
                context.IpAddress,
                eventDetail,
                ct);

            // Só avalia abuso em device novo (ou se quiser, sempre)
            if (isNewDevice)
                await _abuse.EvaluateLicenseAsync(license.Id, ct);

            var token = _tokenService.GenerateToken(license, hardwareId, maxDevices);

            _logger.LogInformation(
                "Licença ativada. Key={Key} HW={HardwareId} NewDevice={IsNew}",
                MaskKey(normalizedKey),
                hardwareId,
                isNewDevice);

            return new ActivateResponse
            {
                Success = true,
                Message = "Ativado com sucesso",
                Token = token,
                ExpiresAt = license.ExpiresAt,
                MaxDevices = maxDevices,
                Status = "Active"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex,
                "Erro ao ativar licença {Key} | HW={HardwareId}",
                MaskKey(normalizedKey),
                hardwareId);
            throw;
        }
    }

    private async Task<bool> ValidateRequestSecurityAsync(
        SecureActivateRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Nonce))
            return false;

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(request.Timestamp);
        var diff = (DateTimeOffset.UtcNow - requestTime).Duration();

        // Janela de 60 segundos
        if (diff > TimeSpan.FromSeconds(60))
            return false;

        if (await _nonceService.IsNonceUsedAsync(request.Nonce))
            return false;

        // Marca o nonce como usado (anti-replay)
        await _nonceService.MarkNonceAsUsedAsync(request.Nonce, TimeSpan.FromMinutes(5));
        return true;
    }

    private static int ResolveMaxDevices(License license)
    {
        if (license.MaxDevices > 0)
            return license.MaxDevices;

        return license.Plan?.MaxDevices ?? 1;
    }

    private static ActivateResponse Fail(
        string message,
        string? status = null,
        int? maxDevices = null)
    {
        return new ActivateResponse
        {
            Success = false,
            Message = message,
            Status = status,
            MaxDevices = maxDevices ?? 0
        };
    }

    private static string MaskKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8)
            return "***";

        return $"{key[..4]}...{key[^4..]}";
    }
}