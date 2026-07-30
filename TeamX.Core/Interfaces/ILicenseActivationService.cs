using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface ILicenseActivationService
{
    Task<ActivateResponse> ActivateAsync(
        SecureActivateRequest request,
        ActivationContext context,
        CancellationToken cancellationToken = default);
}