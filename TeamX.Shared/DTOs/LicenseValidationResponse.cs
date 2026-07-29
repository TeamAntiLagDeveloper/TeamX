namespace TeamX.Shared.DTOs;

/// <summary>
/// Resposta da validação de licença.
/// </summary>
public class LicenseValidationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Active, Expired, Invalid
    public DateTime? ExpiresAt { get; set; }
    public bool HardwareMatched { get; set; }
}