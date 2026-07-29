using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Resend;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string From { get; set; } = "Team AntiLag <noreply@teamantilag.com>";
    public string DownloadUrl { get; set; } = "https://teamantilag.com/teamx";
    public string SupportEmail { get; set; } = "suporte@teamantilag.com";
    public string ProductDisplayName { get; set; } = "TeamX Optimizer";
}

public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IResend resend,
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger)
    {
        _resend = resend;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendLicenseAsync(
        LicenseEmailRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            throw new ArgumentException("CustomerEmail é obrigatório.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.LicenseKey))
            throw new ArgumentException("LicenseKey é obrigatória.", nameof(request));

        var email = new EmailMessage
        {
            From = _options.From,
            Subject = "Sua licença TeamX chegou 🚀"
        };

        email.To.Add(request.CustomerEmail.Trim());

        email.HtmlBody = BuildLicenseEmailHtml(request);

        try
        {
            await _resend.EmailSendAsync(email, ct);

            _logger.LogInformation(
                "E-mail de licença enviado para {Email} | Key: {Key}",
                request.CustomerEmail,
                MaskLicenseKey(request.LicenseKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falha ao enviar e-mail de licença para {Email}",
                request.CustomerEmail);
            throw;
        }
    }

    private string BuildLicenseEmailHtml(LicenseEmailRequest request)
    {
        // Escape para evitar quebra de HTML / XSS se algum campo vier sujo
        var licenseKey = WebUtility.HtmlEncode(request.LicenseKey);
        var productName = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(request.ProductName)
                ? _options.ProductDisplayName
                : request.ProductName);

        var expiration = request.ExpirationDate; // já deve vir formatado do caller
        var year = DateTime.UtcNow.Year;
        var downloadUrl = _options.DownloadUrl;
        var supportEmail = _options.SupportEmail;

        var sb = new StringBuilder();

        sb.Append($$"""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
            </head>
            <body style="font-family: Arial, Helvetica, sans-serif; background:#f4f4f4; padding:30px; margin:0;">
              <div style="max-width:600px; margin:auto; background:white; padding:35px; border-radius:12px;">
                <h1 style="color:#111; text-align:center; margin-top:0;">
                  🚀 Bem-vindo ao TeamX
                </h1>

                <p style="font-size:16px; color:#333; line-height:1.5;">
                  Olá! Obrigado por adquirir o <b>{{productName}}</b>.
                </p>

                <p style="font-size:16px; color:#333; line-height:1.5;">
                  Sua compra foi confirmada e sua licença já está pronta.
                  Agora você pode aproveitar todo o poder de otimização do TeamX.
                </p>

                <div style="background:#111; padding:20px; border-radius:10px; text-align:center; margin:25px 0;">
                  <p style="color:#aaa; margin:0 0 8px 0; font-size:14px;">
                    Sua chave de ativação:
                  </p>
                  <h2 style="color:white; letter-spacing:2px; margin:0; word-break:break-all;">
                    {{licenseKey}}
                  </h2>
                </div>

                <p style="font-size:15px; color:#333; margin:8px 0;">
                  <b>Produto:</b> {{productName}}
                </p>
                <p style="font-size:15px; color:#333; margin:8px 0;">
                  <b>Validade:</b> {{expiration}}
                </p>

                <div style="text-align:center; margin:30px 0;">
                  <a href="{{downloadUrl}}"
                     style="background:#0078ff; color:white; padding:14px 28px;
                            text-decoration:none; border-radius:8px; font-weight:bold;
                            display:inline-block;">
                    Baixar TeamX
                  </a>
                </div>

                <h3 style="color:#111; margin-bottom:10px;">
                  Como ativar sua licença:
                </h3>
                <ol style="color:#333; line-height:1.8; padding-left:20px;">
                  <li>Baixe e instale o TeamX</li>
                  <li>Abra o aplicativo</li>
                  <li>Cole sua chave de ativação</li>
                  <li>Clique em ativar licença</li>
                </ol>

                <hr style="border:none; border-top:1px solid #ddd; margin:30px 0;">

                <p style="font-size:13px; color:#777; text-align:center; margin:0 0 8px 0;">
                  Precisa de ajuda? Fale com o suporte:
                  <a href="mailto:{{supportEmail}}" style="color:#0078ff;">{{supportEmail}}</a>
                </p>
                <p style="font-size:13px; color:#999; text-align:center; margin:0;">
                  © {{year}} Team AntiLag — Todos os direitos reservados.
                </p>
              </div>
            </body>
            </html>
            """);

        return sb.ToString();
    }

    private static string MaskLicenseKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8)
            return "***";

        return $"{key[..4]}...{key[^4..]}";
    }
}