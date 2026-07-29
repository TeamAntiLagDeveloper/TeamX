namespace TeamX.Core.Entities;

/// <summary>
/// Representa um token JWT que foi invalidado (blacklist).
/// Usado para logout, troca de senha, revogação de sessão, etc.
/// </summary>
public class RevokedToken
{
    public Guid Id { get; set; }

    /// <summary>
    /// JWT ID (jti claim). Identificador único do token.
    /// </summary>
    public string Jti { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora em que o token foi revogado (UTC).
    /// </summary>
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data/hora de expiração original do token (UTC).
    /// Permite limpar registros antigos automaticamente.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    // Opcional, mas recomendado:
    // public Guid? UserId { get; set; }          // para consultas por usuário
    // public string? Reason { get; set; }        // "Logout", "PasswordChanged", etc.
}