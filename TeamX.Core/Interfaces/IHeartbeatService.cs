namespace TeamX.Core.Interfaces;

public interface IHeartbeatService
{
    Task<bool> RecordHeartbeatAsync(string token, string hardwareFingerprint);
    Task<bool> IsDeviceActiveAsync(string token, string hardwareFingerprint);
}