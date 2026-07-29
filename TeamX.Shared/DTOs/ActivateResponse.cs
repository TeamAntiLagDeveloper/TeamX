namespace TeamX.Shared.DTOs;

/// <summary>
/// Resposta da ativação de licença.
/// </summary>
public class ActivateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty; // JWT
    public DateTime ExpiresAt { get; set; }
    public int MaxDevices { get; set; }
    public string Status { get; set; } = string.Empty;
}