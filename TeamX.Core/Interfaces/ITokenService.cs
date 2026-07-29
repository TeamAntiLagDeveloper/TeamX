using TeamX.Core.Entities;
using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável pela geração, validação e revogação de tokens de licença.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera um token JWT para a licença e dispositivo informados.
    /// </summary>
    string GenerateToken(
        License license,
        string hardwareFingerprint,
        int maxDevices);

    /// <summary>
    /// Valida um token e o fingerprint do hardware.
    /// </summary>
    Task<TokenValidationResponse> ValidateTokenAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoga (invalida) um token.
    /// </summary>
    Task RevokeTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um token já foi revogado.
    /// </summary>
    Task<bool> IsTokenRevokedAsync(
        string token,
        CancellationToken cancellationToken = default);
}