namespace TeamX.App.Models;

public class ActivateLicenseResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Token { get; set; }
}