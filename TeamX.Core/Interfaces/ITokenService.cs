using TeamX.Core.Entities;
using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(
        License license,
        string hardwareFingerprint,
        int maxDevices);

    Task<TokenValidationResponse> ValidateTokenAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> IsTokenRevokedAsync(
        string token,
        CancellationToken cancellationToken = default);
}