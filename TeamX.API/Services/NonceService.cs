using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class NonceService : INonceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NonceService> _logger;

    // Probabilidade de rodar cleanup neste request (~2%)
    private const double CleanupProbability = 0.02;
    private const int CleanupBatchSize = 200;

    public NonceService(
        ApplicationDbContext context,
        ILogger<NonceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsNonceUsedAsync(
        string nonce,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nonce))
            return true; // trata como já usado → rejeita a request

        // Cleanup ocasional (não a cada request)
        if (Random.Shared.NextDouble() < CleanupProbability)
            await CleanupExpiredAsync(ct);

        return await _context.UsedNonces
            .AsNoTracking()
            .AnyAsync(x =>
                x.Nonce == nonce &&
                x.ExpiresAt > DateTime.UtcNow,
                ct);
    }

    public async Task MarkNonceAsUsedAsync(
        string nonce,
        TimeSpan expiration,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nonce))
            return;

        if (expiration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(expiration), "Expiration deve ser positivo.");

        var entity = new UsedNonce
        {
            Nonce = nonce.Trim(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiration)
        };

        try
        {
            _context.UsedNonces.Add(entity);
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Race condition: outro request já inseriu o mesmo nonce.
            // Índice único garante a proteção — apenas ignora.
            _context.Entry(entity).State = EntityState.Detached;

            _logger.LogDebug(
                "Nonce já estava marcado como usado (race). Nonce={Nonce}",
                MaskNonce(nonce));
        }
    }

    /// <summary>
    /// Remove nonces expirados em lote.
    /// Pode ser chamado pelo serviço ou por um BackgroundService dedicado.
    /// </summary>
    public async Task CleanupExpiredAsync(CancellationToken ct = default)
    {
        try
        {
            // EF Core 7+: deleta direto no banco, sem carregar entidades
            var deleted = await _context.UsedNonces
                .Where(x => x.ExpiresAt < DateTime.UtcNow)
                .Take(CleanupBatchSize)
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
            {
                _logger.LogDebug(
                    "Cleanup de nonces: {Count} registros expirados removidos",
                    deleted);
            }
        }
        catch (Exception ex)
        {
            // Cleanup nunca deve derrubar o fluxo principal
            _logger.LogWarning(ex, "Falha no cleanup de nonces expirados");
        }
    }

    private static string MaskNonce(string nonce)
    {
        if (string.IsNullOrEmpty(nonce) || nonce.Length < 8)
            return "***";

        return $"{nonce[..4]}...{nonce[^4..]}";
    }
}