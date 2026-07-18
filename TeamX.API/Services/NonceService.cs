using Microsoft.Extensions.Caching.Memory;
using TeamX.Core.Interfaces;

namespace TeamX.API.Services;

public class NonceService : INonceService
{
    private readonly IMemoryCache _cache;

    public NonceService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<bool> IsNonceUsedAsync(string nonce)
    {
        return Task.FromResult(_cache.TryGetValue(nonce, out _));
    }

    public Task MarkNonceAsUsedAsync(string nonce, TimeSpan expiration)
    {
        _cache.Set(nonce, true, expiration);
        return Task.CompletedTask;
    }
}