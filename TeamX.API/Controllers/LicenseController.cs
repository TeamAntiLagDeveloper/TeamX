using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Text;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Controllers;

/// <summary>
/// Gerencia ativação, validação e heartbeat de licenças.
/// Criação de licença só via admin key ou webhook (não público).
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
    private readonly IConfiguration _configuration;
    private readonly ILogger<LicenseController> _logger;

    public LicenseController(
        ILicenseActivationService activationService,
        IHeartbeatService heartbeatService,
        ITokenService tokenService,
        ILicenseService licenseService,
        IConfiguration configuration,
        ILogger<LicenseController> logger)
    {
        _activationService = activationService;
        _heartbeatService = heartbeatService;
        _tokenService = tokenService;
        _licenseService = licenseService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Ativa uma licença vinculando-a a um hardware fingerprint.
    /// </summary>
    [HttpPost("activate")]
    [EnableRateLimiting("activate")]
    [ProducesResponseType(typeof(ActivateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ActivateResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(
        [FromBody] SecureActivateRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.LicenseKey) ||
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
    [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken(
        [FromBody] TokenValidationRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.HardwareFingerprint))
        {
            return BadRequest(new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
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
    [EnableRateLimiting("heartbeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Heartbeat(
        [FromBody] SecureHeartbeatRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Token) ||
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
    /// Cria uma nova licença (somente com X-Admin-ApiKey).
    /// Em produção normal as licenças vêm do webhook Eremby.
    /// </summary>
    [HttpPost("create")]
    [EnableRateLimiting("activate")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLicenseRequest? request,
        CancellationToken cancellationToken)
    {
        if (!ValidateAdminApiKey())
        {
            _logger.LogWarning(
                "Tentativa de create sem admin key. IP: {Ip}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Unauthorized(new
            {
                Success = false,
                Message = "Não autorizado."
            });
        }

        if (request is null ||
            request.CustomerId == Guid.Empty ||
            request.ProductId == Guid.Empty ||
            request.PlanId == Guid.Empty)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "CustomerId, ProductId e PlanId são obrigatórios."
            });
        }

        try
        {
            var license = await _licenseService.CreateLicenseAsync(request, cancellationToken);

            var response = new
            {
                Success = true,
                LicenseKey = license.Key,
                LicenseId = license.Id,
                Status = license.Status,
                ExpiresAt = license.ExpiresAt
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private bool ValidateAdminApiKey()
    {
        var expected = _configuration["Admin:ApiKey"];
        if (string.IsNullOrWhiteSpace(expected) || expected.Length < 16)
            return false;

        var received = Request.Headers["X-Admin-ApiKey"].FirstOrDefault();
        if (string.IsNullOrEmpty(received))
            return false;

        var receivedBytes = Encoding.UTF8.GetBytes(received);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return receivedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(receivedBytes, expectedBytes);
    }
}