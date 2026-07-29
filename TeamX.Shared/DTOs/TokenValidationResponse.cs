namespace TeamX.Shared.DTOs;

/// <summary>
/// Resposta da validação de token.
/// </summary>
public class TokenValidationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsValid { get; set; }

    /// <summary>
    /// Versão mínima exigida pelo servidor.
    /// </summary>
    public string? MinAppVersion { get; set; }

    /// <summary>
    /// True se o cliente está abaixo da versão mínima.
    /// </summary>
    public bool ForceUpdate { get; set; }
}