namespace TeamX.API.DTOs.License;

public class CreateLicenseResponse
{
    public bool Success { get; set; }

    public string LicenseKey { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }
}