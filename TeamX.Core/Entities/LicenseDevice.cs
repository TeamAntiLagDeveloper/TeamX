namespace TeamX.Core.Entities;

public class LicenseDevice
{
    public Guid Id { get; set; }


    public int LicenseId { get; set; }

    public License License { get; set; } = null!;


    public string HardwareId { get; set; } = null!;

    public string ComputerName { get; set; } = null!;

    public string WindowsVersion { get; set; } = null!;

    public string IpAddress { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime FirstSeen { get; set; }

    public DateTime LastSeen { get; set; }
}