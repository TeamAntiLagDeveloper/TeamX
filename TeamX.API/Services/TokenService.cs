using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationDays { get; set; } = 7;
}

public class TokenService : ITokenService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtOptions _jwt;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    private readonly TokenValidationParameters _validationParameters;
    private readonly JwtSecurityTokenHandler _handler = new();
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _credentials;

    public TokenService(
        ApplicationDbContext context,
        IOptions<JwtOptions> jwtOptions,
        IConfiguration configuration,
        ILogger<TokenService> logger)
    {
        _context = context;
        _jwt = jwtOptions.Value;
        _configuration = configuration;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_jwt.Secret) || _jwt.Secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret deve ter pelo menos 32 caracteres.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        _credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwt.Issuer,
            ValidAudience = _jwt.Audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    public string GenerateToken(License license, string hardwareFingerprint, int maxDevices)
    {
        ArgumentNullException.ThrowIfNull(license);

        if (string.IsNullOrWhiteSpace(hardwareFingerprint))
            throw new ArgumentException("HardwareFingerprint é obrigatório.", nameof(hardwareFingerprint));

        var now = DateTime.UtcNow;
        var jti = Guid.NewGuid().ToString("N");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, license.Key),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("HardwareFingerprint", hardwareFingerprint.Trim()),
            new Claim("LicenseId", license.Id.ToString()),
            new Claim("MaxDevices", maxDevices.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddDays(_jwt.ExpirationDays),
            signingCredentials: _credentials);

        return _handler.WriteToken(token);
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hardwareFingerprint))
            return Fail("Token ou dispositivo inválido.");

        try
        {
            if (await IsTokenRevokedAsync(token, ct))
                return Fail("Token revogado.");

            var principal = _handler.ValidateToken(token, _validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt)
                return Fail("Token inválido.");

            var storedHardware = GetClaim(principal, "HardwareFingerprint");
            if (!string.Equals(storedHardware, hardwareFingerprint.Trim(), StringComparison.OrdinalIgnoreCase))
                return Fail("Dispositivo não autorizado.");

            if (!int.TryParse(GetClaim(principal, "LicenseId"), out var licenseId))
                return Fail("Token inválido.");

            var license = await _context.Licenses
                .AsNoTracking()
                .Where(x => x.Id == licenseId)
                .Select(x => new { x.Id, x.Status, x.ExpiresAt })
                .FirstOrDefaultAsync(ct);

            if (license is null)
                return Fail("Licença não encontrada.");

            if (license.Status != "Active")
            {
                return Fail(
                    $"Licença {license.Status}.",
                    status: license.Status,
                    expiresAt: license.ExpiresAt);
            }

            if (license.ExpiresAt <= DateTime.UtcNow)
            {
                return Fail(
                    "Licença expirada.",
                    status: "Expired",
                    expiresAt: license.ExpiresAt);
            }

            var deviceActive = await _context.LicenseDevices
                .AsNoTracking()
                .AnyAsync(d =>
                    d.LicenseId == licenseId &&
                    d.IsActive &&
                    d.HardwareId == hardwareFingerprint.Trim(),
                    ct);

            if (!deviceActive)
                return Fail("Dispositivo não autorizado.");

            var minVersion = _configuration["App:MinClientVersion"] ?? "1.0.0";

            return new TokenValidationResponse
            {
                Success = true,
                IsValid = true,
                Status = "Active",
                ExpiresAt = license.ExpiresAt,
                LicenseId = licenseId,
                Message = "Token válido.",
                MinAppVersion = minVersion,
                ForceUpdate = false
            };
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogDebug(ex, "Falha na validação do JWT");
            return Fail("Token inválido ou expirado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao validar token");
            return Fail("Token inválido ou expirado.");
        }
    }

    public async Task RevokeTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        try
        {
            var jwt = _handler.ReadJwtToken(token);
            var jti = jwt.Id;

            if (string.IsNullOrEmpty(jti))
                return;

            var exists = await _context.RevokedTokens
                .AsNoTracking()
                .AnyAsync(x => x.Jti == jti, ct);

            if (exists)
                return;

            _context.RevokedTokens.Add(new RevokedToken
            {
                Id = Guid.NewGuid(),
                Jti = jti,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = jwt.ValidTo
            });

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Token revogado. Jti={Jti}", jti);
        }
        catch (Exception ex)
        {
            // Token malformado — não interrompe o fluxo do caller
            _logger.LogDebug(ex, "Não foi possível revogar token (malformado?)");
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return true;

        try
        {
            var jwt = _handler.ReadJwtToken(token);
            var jti = jwt.Id;

            if (string.IsNullOrEmpty(jti))
                return true;

            return await _context.RevokedTokens
                .AsNoTracking()
                .AnyAsync(x => x.Jti == jti, ct);
        }
        catch
        {
            return true; // token ilegível → trata como revogado
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static string? GetClaim(ClaimsPrincipal principal, string type)
        => principal.FindFirstValue(type);

    private static TokenValidationResponse Fail(
        string message,
        string? status = null,
        DateTime? expiresAt = null)
    {
        return new TokenValidationResponse
        {
            Success = false,
            IsValid = false,
            Message = message,
            Status = status,
            ExpiresAt = expiresAt
        };
    }
}