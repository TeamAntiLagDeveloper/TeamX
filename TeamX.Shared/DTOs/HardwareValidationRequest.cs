namespace TeamX.Shared.DTOs;

/// <summary>
/// Request de validação de hardware.
/// </summary>
public class HardwareValidationRequest
{
    public string HardwareId { get; set; } = string.Empty;
    public string? ComputerName { get; set; }
    public string? WindowsVersion { get; set; }
    public string? IpAddress { get; set; }
}