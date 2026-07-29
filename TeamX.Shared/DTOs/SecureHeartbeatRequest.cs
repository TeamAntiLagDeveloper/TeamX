namespace TeamX.Shared.DTOs;

/// <summary>
/// Request seguro de heartbeat.
/// </summary>
public class SecureHeartbeatRequest
{
    public string Token { get; set; } = string.Empty;
    public string HardwareFingerprint { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}