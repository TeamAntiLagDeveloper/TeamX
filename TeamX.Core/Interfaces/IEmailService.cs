using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface IEmailService
{
    Task SendLicenseAsync(
        LicenseEmailRequest request,
        CancellationToken cancellationToken = default);
}