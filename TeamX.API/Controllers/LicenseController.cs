using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Controllers;

/// <summary>
/// Gerencia ativação, validação e criação de licenças.
/// </summary>
[ApiController]
[Route("api/license")]
[Produces("application/json")]
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

    /// <summary>
    /// Ativa uma licença vinculando-a a um hardware fingerprint.
    /// </summary>
    [HttpPost("activate")]
    [EnableRateLimiting("activate")]
    [ProducesResponseType(typeof(ActivateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ActivateResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(
        [FromBody] SecureActivateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseKey) ||
            string.IsNullOrWhiteSpace(request.HardwareFingerprint))
        {
            return BadRequest(new ActivateResponse
            {
                Success = false,
                Message = "LicenseKey e HardwareFingerprint são obrigatórios."
            });
        }

        var context = new ActivationContext
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var response = await _activationService.ActivateAsync(request, context, cancellationToken);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }

    /// <summary>
    /// Valida um token de licença.
    /// </summary>
    [HttpPost("validate-token")]
    [EnableRateLimiting("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken(
        [FromBody] TokenValidationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.HardwareFingerprint))
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Token e HardwareFingerprint são obrigatórios."
            });
        }

        var result = await _tokenService.ValidateTokenAsync(
            request.Token,
            request.HardwareFingerprint,
            cancellationToken);

        return result is { Success: true, IsValid: true }
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Registra um heartbeat para manter a sessão da licença ativa.
    /// </summary>
    [HttpPost("heartbeat")]
    [EnableRateLimiting("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Heartbeat(
        [FromBody] SecureHeartbeatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.HardwareFingerprint))
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Token e HardwareFingerprint são obrigatórios."
            });
        }

        var success = await _heartbeatService.RecordHeartbeatAsync(
            request.Token,
            request.HardwareFingerprint,
            cancellationToken);

        return success
            ? Ok(new { Success = true, Message = "Heartbeat registrado com sucesso." })
            : BadRequest(new { Success = false, Message = "Token inválido ou expirado." });
    }

    /// <summary>
    /// Cria uma nova licença.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLicenseRequest request,
        CancellationToken cancellationToken)
    {
        var license = await _licenseService.CreateLicenseAsync(request, cancellationToken);

        var response = new
        {
            Success = true,
            LicenseKey = license.Key,
            Status = license.Status,
            ExpiresAt = license.ExpiresAt
        };

        // Retorna 201 Created (mais semântico para criação de recurso)
        return CreatedAtAction(nameof(Create), response);
    }
}