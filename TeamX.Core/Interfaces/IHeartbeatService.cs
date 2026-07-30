namespace TeamX.Core.Interfaces;

public interface IHeartbeatService
{
    Task<bool> RecordHeartbeatAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken cancellationToken = default);

    Task<bool> IsDeviceActiveAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken cancellationToken = default);
}