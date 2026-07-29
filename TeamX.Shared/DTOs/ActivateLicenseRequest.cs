namespace TeamX.Shared.DTOs;

/// <summary>
/// Request de ativação de licença (versão legada).
/// </summary>
public class ActivateLicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;
    public string HardwareId { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string WindowsVersion { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}