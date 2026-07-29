using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável pelo login de licenças.
/// </summary>
public interface ILicenseLoginService
{
    /// <summary>
    /// Realiza o login de uma licença e retorna o token de acesso.
    /// </summary>
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}