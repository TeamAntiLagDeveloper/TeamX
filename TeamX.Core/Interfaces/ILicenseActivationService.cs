using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável pela ativação segura de licenças.
/// </summary>
public interface ILicenseActivationService
{
    /// <summary>
    /// Realiza a ativação de uma licença com validação de assinatura e contexto.
    /// </summary>
    Task<ActivateResponse> ActivateAsync(
        SecureActivateRequest request,
        ActivationContext context,
        CancellationToken cancellationToken = default);
}