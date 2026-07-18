namespace TeamX.Core.Interfaces;

public interface INonceService
{
    Task<bool> IsNonceUsedAsync(string nonce);
    Task MarkNonceAsUsedAsync(string nonce, TimeSpan expiration);
}