namespace TeamX.Core.Entities;

/// <summary>
/// Nonce já usado (anti-replay na ativação).
/// </summary>
public class UsedNonce
{
    public Guid Id { get; set; }

    public string Nonce { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }
}