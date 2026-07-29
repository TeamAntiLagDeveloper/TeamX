using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável pelo envio de e-mails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia o e-mail de ativação/entrega da licença.
    /// </summary>
    Task SendLicenseAsync(
        LicenseEmailRequest request,
        CancellationToken cancellationToken = default);
}