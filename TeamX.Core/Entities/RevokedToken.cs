namespace TeamX.Core.Entities;

/// <summary>
/// JWT invalidado (blacklist por jti).
/// </summary>
public class RevokedToken
{
    public Guid Id { get; set; }

    /// <summary>
    /// Claim jti do JWT.
    /// </summary>
    public string Jti { get; set; } = string.Empty;

    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expiração original do token (para cleanup).
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}