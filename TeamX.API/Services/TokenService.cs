using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public TokenService(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateToken(License license, string hardwareFingerprint, int maxDevices)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, license.Key),
            new Claim("HardwareFingerprint", hardwareFingerprint),
            new Claim("LicenseId", license.Id.ToString()),
            new Claim("MaxDevices", maxDevices.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(string token, string hardwareFingerprint)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var storedHardware = jwtToken.Claims
                .FirstOrDefault(c => c.Type == "HardwareFingerprint")?.Value;

            if (storedHardware != hardwareFingerprint)
            {
                return new TokenValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Message = "Dispositivo não autorizado."
                };
            }

            var licenseId = int.Parse(
                jwtToken.Claims.First(c => c.Type == "LicenseId").Value);

            var license = await _context.Licenses
                .FirstOrDefaultAsync(x => x.Id == licenseId);

            if (license == null)
            {
                return new TokenValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Message = "Licença não encontrada."
                };
            }

            if (license.Status != "Active")
            {
                return new TokenValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Status = license.Status,
                    ExpiresAt = license.ExpiresAt,
                    Message = $"Licença {license.Status}."
                };
            }

            if (license.ExpiresAt <= DateTime.UtcNow)
            {
                return new TokenValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Status = "Expired",
                    ExpiresAt = license.ExpiresAt,
                    Message = "Licença expirada."
                };
            }

            return new TokenValidationResponse
            {
                Success = true,
                IsValid = true,
                Status = "Active",
                ExpiresAt = license.ExpiresAt,
                Message = "Token válido."
            };
        }
        catch
        {
            return new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
                Message = "Token inválido ou expirado."
            };
        }
    }

    public Task RevokeTokenAsync(string token)
    {
        return Task.CompletedTask;
    }

    public Task<bool> IsTokenRevokedAsync(string token)
    {
        return Task.FromResult(false);
    }
}