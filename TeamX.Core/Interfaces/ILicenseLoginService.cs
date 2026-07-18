using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface ILicenseLoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}