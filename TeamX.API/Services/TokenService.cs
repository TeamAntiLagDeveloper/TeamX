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

    public Task<TokenValidationResponse> ValidateTokenAsync(string token, string hardwareFingerprint)
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
                return Task.FromResult(new TokenValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Message = "Dispositivo não autorizado"
                });
            }

            return Task.FromResult(new TokenValidationResponse
            {
                Success = true,
                IsValid = true,
                Message = "Token válido"
            });
        }
        catch
        {
            return Task.FromResult(new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
                Message = "Token inválido ou expirado"
            });
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