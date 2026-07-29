namespace TeamX.Shared.DTOs;

/// <summary>
/// Request de validação de token.
/// </summary>
public class TokenValidationRequest
{
    public string Token { get; set; } = string.Empty;
    public string HardwareFingerprint { get; set; } = string.Empty;
    public string? ExecutableHash { get; set; }
}