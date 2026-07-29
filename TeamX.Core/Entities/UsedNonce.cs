namespace TeamX.Core.Entities;

/// <summary>
/// Armazena nonces já utilizados para prevenir ataques de replay
/// (ex: webhooks, autenticação one-time, signatures).
/// </summary>
public class UsedNonce
{
    public Guid Id { get; set; }

    /// <summary>
    /// Valor único do nonce (normalmente um valor aleatório ou hash).
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora em que o nonce foi registrado (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data/hora em que o nonce expira e pode ser removido (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}