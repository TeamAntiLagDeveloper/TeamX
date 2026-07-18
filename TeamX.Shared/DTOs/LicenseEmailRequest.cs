namespace TeamX.Shared.DTOs;

public class LicenseEmailRequest
{
    public string CustomerEmail { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string LicenseKey { get; set; } = null!;

    public DateTime ExpirationDate { get; set; }

    public string DownloadLink { get; set; } = null!;

    public string ActivationInstructions { get; set; } = null!;
}