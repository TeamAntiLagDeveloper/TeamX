namespace TeamX.Shared.DTOs;

public class HeartbeatRequest
{
    public string LicenseKey { get; set; } = string.Empty;
    public string HardwareId { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}