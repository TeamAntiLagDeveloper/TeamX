using Microsoft.AspNetCore.Mvc;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Controllers;

[ApiController]
[Route("api/license")]
public class LicenseController : ControllerBase
{
    private readonly ILicenseActivationService _activationService;
    private readonly IHeartbeatService _heartbeatService;
    private readonly ITokenService _tokenService;
    private readonly ILicenseService _licenseService;

    public LicenseController(
        ILicenseActivationService activationService,
        IHeartbeatService heartbeatService,
        ITokenService tokenService,
        ILicenseService licenseService)
    {
        _activationService = activationService;
        _heartbeatService = heartbeatService;
        _tokenService = tokenService;
        _licenseService = licenseService;
    }

    // =====================================================
    // ATIVAÇÃO SEGURA (Novo fluxo principal)
    // =====================================================
    [HttpPost("activate")]
    public async Task<IActionResult> Activate(
        [FromBody] SecureActivateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseKey) ||
            string.IsNullOrWhiteSpace(request.HardwareFingerprint))
        {
            return BadRequest(new ActivateResponse
            {
                Success = false,
                Message = "Dados inválidos"
            });
        }


        var context = new ActivationContext
        {
            IpAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString()
        };


        var response =
            await _activationService.ActivateAsync(
                request,
                context);


        if (!response.Success)
            return BadRequest(response);


        return Ok(response);
    }

    // =====================================================
    // VALIDAÇÃO POR TOKEN (usado em inicialização e heartbeats)
    // =====================================================
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
    {
        var result = await _tokenService.ValidateTokenAsync(request.Token, request.HardwareFingerprint);

        if (!result.Success || !result.IsValid)
            return BadRequest(result);

        return Ok(result);
    }

    // =====================================================
    // HEARTBEAT (agora usa Token)
    // =====================================================
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] SecureHeartbeatRequest request)
    {
        bool success = await _heartbeatService.RecordHeartbeatAsync(
            request.Token,
            request.HardwareFingerprint);

        return Ok(new
        {
            success,
            message = success ? "Heartbeat registrado" : "Token inválido ou expirado"
        });
    }

    // =====================================================
    // CRIAR LICENÇA (mantido para webhook)
    // =====================================================
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateLicenseRequest request)
    {
        var license = await _licenseService.CreateLicenseAsync(request);

        return Ok(new
        {
            success = true,
            licenseKey = license.Key,
            status = license.Status,
            expiresAt = license.ExpiresAt
        });
    }
}