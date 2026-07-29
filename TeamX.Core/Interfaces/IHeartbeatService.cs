namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável por registrar e verificar a atividade de dispositivos (heartbeat).
/// </summary>
public interface IHeartbeatService
{
    /// <summary>
    /// Registra um heartbeat de um dispositivo.
    /// </summary>
    Task<bool> RecordHeartbeatAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um dispositivo ainda está ativo.
    /// </summary>
    Task<bool> IsDeviceActiveAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken cancellationToken = default);
}