namespace TeamX.Shared.DTOs;

/// <summary>
/// Contexto adicional da ativação (ex: IP de origem).
/// </summary>
public class ActivationContext
{
    public string? IpAddress { get; set; }
}