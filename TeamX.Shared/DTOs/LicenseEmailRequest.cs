namespace TeamX.Shared.DTOs;

/// <summary>
/// Dados necessários para envio do e-mail de licença.
/// </summary>
public class LicenseEmailRequest
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public string DownloadLink { get; set; } = string.Empty;
    public string ActivationInstructions { get; set; } = string.Empty;
}