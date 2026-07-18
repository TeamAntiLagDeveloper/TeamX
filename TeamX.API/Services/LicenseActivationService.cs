using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class LicenseActivationService : ILicenseActivationService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly INonceService _nonceService;
    private readonly ISignatureService _signatureService;
    private readonly IConfiguration _configuration;

    public LicenseActivationService(
        ApplicationDbContext context,
        ITokenService tokenService,
        INonceService nonceService,
        ISignatureService signatureService,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _nonceService = nonceService;
        _signatureService = signatureService;
        _configuration = configuration;
    }

    public async Task<ActivateResponse> ActivateAsync(SecureActivateRequest request, ActivationContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LicenseKey))
            return new ActivateResponse { Success = false, Message = "Dados inválidos" };

        if (!await ValidateRequestSecurity(request))
            return new ActivateResponse { Success = false, Message = "Requisição inválida ou adulterada" };

        var license = await _context.Licenses
            .Include(x => x.Plan)
            .Include(x => x.Devices)
            .FirstOrDefaultAsync(x => x.Key.Trim().ToUpper() == request.LicenseKey.Trim().ToUpper());

        if (license == null)
            return new ActivateResponse { Success = false, Message = "Licença não encontrada" };

        if (license.Status != "Active")
            return new ActivateResponse { Success = false, Message = "Licença não está ativa", Status = license.Status };

        if (license.ExpiresAt < DateTime.UtcNow)
            return new ActivateResponse { Success = false, Message = "Licença expirada", Status = "Expired" };

        var plan = license.Plan;

        var activeDevices = license.Devices
            .Count(x => x.IsActive);


        if (activeDevices >= plan.MaxDevices)
        {
            return new ActivateResponse
            {
                Success = false,
                Message = "Limite de dispositivos atingido"
            };
        }

        var existingDevice = license.Devices.FirstOrDefault(d =>
            d.HardwareId.Equals(request.HardwareFingerprint, StringComparison.OrdinalIgnoreCase));

        if (existingDevice != null)
        {
            existingDevice.LastSeen = DateTime.UtcNow;

            existingDevice.ComputerName =
                request.ComputerName ?? "";

            existingDevice.WindowsVersion =
                request.WindowsVersion ?? "";

            existingDevice.IpAddress =
                context.IpAddress ?? "";

            existingDevice.IsActive = true;


            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(license, request.HardwareFingerprint, plan.MaxDevices);

            return new ActivateResponse
            {
                Success = true,
                Message = "Dispositivo já registrado",
                Token = token,
                ExpiresAt = license.ExpiresAt,
                MaxDevices = plan.MaxDevices
            };
        }

        // Novo dispositivo
        var newDevice = new LicenseDevice
        {
            Id = Guid.NewGuid(),

            LicenseId = license.Id,

            HardwareId = request.HardwareFingerprint,

            ComputerName =
                request.ComputerName ?? "",

            WindowsVersion =
                request.WindowsVersion ?? "",

            IpAddress =
                context.IpAddress ?? "",

            FirstSeen = DateTime.UtcNow,

            LastSeen = DateTime.UtcNow,

            IsActive = true
        };

        _context.LicenseDevices.Add(newDevice);
        await _context.SaveChangesAsync();

        var jwtToken = _tokenService.GenerateToken(license, request.HardwareFingerprint, plan.MaxDevices);

        return new ActivateResponse
        {
            Success = true,
            Message = "Dispositivo ativado com sucesso",
            Token = jwtToken,
            ExpiresAt = license.ExpiresAt,
            MaxDevices = plan.MaxDevices,
            Status = "Active"
        };
    }

    private async Task<bool> ValidateRequestSecurity(SecureActivateRequest request)
    {
        var requestTime = DateTimeOffset.FromUnixTimeSeconds(request.Timestamp);

        if (DateTimeOffset.UtcNow - requestTime > TimeSpan.FromSeconds(60))
            throw new Exception("TIMESTAMP INVÁLIDO");

        if (await _nonceService.IsNonceUsedAsync(request.Nonce))
            throw new Exception("NONCE REPETIDO");

        await _nonceService.MarkNonceAsUsedAsync(request.Nonce, TimeSpan.FromMinutes(5));

        var secret = _configuration["Security:RequestSigningSecret"] ?? "";

        var expected = _signatureService.GenerateSignature(request, secret);

        throw new Exception($"""
SECRET: {secret}

EXPECTED:
{expected}

RECEIVED:
{request.Signature}

MATCH:
{expected.Equals(request.Signature, StringComparison.OrdinalIgnoreCase)}
""");
    }
}