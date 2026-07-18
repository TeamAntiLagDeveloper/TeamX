using System.Net;
using System.Net.Mail;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class EmailService : IEmailService
{
    public async Task SendLicenseAsync(
        LicenseEmailRequest request)
    {
        using var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                "SEU_EMAIL",
                "SUA_SENHA_APP"
            ),
            EnableSsl = true
        };


        var mail = new MailMessage();

        mail.From = new MailAddress(
            "SEU_EMAIL",
            "TeamX"
        );

        mail.To.Add(request.CustomerEmail);

        mail.Subject = "Sua licença TeamX foi liberada";


        mail.Body = $@"
Olá!

Obrigado por adquirir o {request.ProductName}.

Sua licença foi criada com sucesso.

Produto:
{request.ProductName}


Chave de ativação:

{request.LicenseKey}


Data de expiração:

{request.ExpirationDate:dd/MM/yyyy}


Download:

{request.DownloadLink}


Como ativar:

{request.ActivationInstructions}


Equipe TeamX
";


        await smtp.SendMailAsync(mail);
    }
}