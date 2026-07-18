using TeamX.Core.Entities;
using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(License license, string hardwareFingerprint, int maxDevices);
    Task<TokenValidationResponse> ValidateTokenAsync(string token, string hardwareFingerprint);
    Task RevokeTokenAsync(string token);
    Task<bool> IsTokenRevokedAsync(string token);
}