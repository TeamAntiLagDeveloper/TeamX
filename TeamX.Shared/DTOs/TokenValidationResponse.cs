namespace TeamX.Shared.DTOs;

public class TokenValidationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsValid { get; set; }
}