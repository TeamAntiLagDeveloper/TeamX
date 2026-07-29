namespace TeamX.Shared.DTOs.Health;

/// <summary>
/// Resposta do endpoint de health check.
/// </summary>
public class HealthResponse
{
    public string Name { get; set; } = "TeamX API";
    public string Version { get; set; } = "1.0.0";
    public string Status { get; set; } = "Online";
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;

    public DateTime Timestamp { get; set; }
}