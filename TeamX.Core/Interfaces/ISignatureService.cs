using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável por geração e validação de assinaturas digitais.
/// </summary>
public interface ISignatureService
{
    /// <summary>
    /// Gera a assinatura de uma requisição de ativação.
    /// </summary>
    string GenerateSignature(
        SecureActivateRequest request,
        string secret);

    /// <summary>
    /// Valida a assinatura de uma requisição de ativação.
    /// </summary>
    bool ValidateSignature(
        SecureActivateRequest request,
        string secret);
}