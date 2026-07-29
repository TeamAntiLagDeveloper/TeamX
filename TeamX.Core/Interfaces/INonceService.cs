namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável pelo controle de nonces (prevenção de replay attacks).
/// </summary>
public interface INonceService
{
    /// <summary>
    /// Verifica se um nonce já foi utilizado.
    /// </summary>
    Task<bool> IsNonceUsedAsync(
        string nonce,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca um nonce como utilizado, com tempo de expiração.
    /// </summary>
    Task MarkNonceAsUsedAsync(
        string nonce,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}