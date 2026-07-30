namespace TeamX.Core.Interfaces;

public interface INonceService
{
    Task<bool> IsNonceUsedAsync(
        string nonce,
        CancellationToken cancellationToken = default);

    Task MarkNonceAsUsedAsync(
        string nonce,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}