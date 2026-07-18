namespace TeamX.Shared.DTOs;

public class ActivateLicenseRequest
{
    public string LicenseKey { get; set; } = "";

    public string HardwareId { get; set; } = "";

    public string? ComputerName { get; set; }

    public string? WindowsVersion { get; set; }

    public string? IpAddress { get; set; }
}