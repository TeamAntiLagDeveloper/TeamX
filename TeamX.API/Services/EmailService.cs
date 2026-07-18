using Resend;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class EmailService : IEmailService
{
    private readonly IResend _resend;

    public EmailService(
        IResend resend)
    {
        _resend = resend;
    }


    public async Task SendLicenseAsync(
        LicenseEmailRequest request)
    {

        var email = new EmailMessage();

        email.From =
            "Team AntiLag <noreply@teamantilag.com>";

        email.To.Add(
            request.CustomerEmail);

        email.Subject =
            "Sua licença TeamX chegou 🚀";


        email.HtmlBody =
$"""
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
</head>

<body style="font-family: Arial, Helvetica, sans-serif; background:#f4f4f4; padding:30px;">

<div style="max-width:600px; margin:auto; background:white; padding:35px; border-radius:12px;">

<h1 style="color:#111; text-align:center;">
🚀 Bem-vindo ao TeamX
</h1>

<p style="font-size:16px; color:#333;">
Olá! Obrigado por adquirir o <b>TeamX Optimizer</b>.
</p>

<p style="font-size:16px; color:#333;">
Sua compra foi confirmada e sua licença já está pronta.
Agora você pode aproveitar todo o poder de otimização do TeamX.
</p>


<div style="background:#111; padding:20px; border-radius:10px; text-align:center; margin:25px 0;">

<p style="color:#aaa; margin:0;">
Sua chave de ativação:
</p>

<h2 style="color:white; letter-spacing:2px;">
{request.LicenseKey}
</h2>

</div>


<p style="font-size:15px;">
<b>Produto:</b> {request.ProductName}
</p>

<p style="font-size:15px;">
<b>Validade:</b> {request.ExpirationDate}
</p>


<div style="text-align:center; margin:30px 0;">

<a href="https://teamantilag.com/teamx"
style="
background:#0078ff;
color:white;
padding:14px 28px;
text-decoration:none;
border-radius:8px;
font-weight:bold;
">
Baixar TeamX
</a>

</div>


<h3 style="color:#111;">
Como ativar sua licença:
</h3>

<ol style="color:#333; line-height:1.8;">
<li>Baixe e instale o TeamX</li>
<li>Abra o aplicativo</li>
<li>Cole sua chave de ativação</li>
<li>Clique em ativar licença</li>
</ol>


<hr style="border:none;border-top:1px solid #ddd;margin:30px 0;">


<p style="font-size:13px;color:#777;text-align:center;">
Precisa de ajuda? Entre em contato com o suporte Team AntiLag.
</p>

<p style="font-size:13px;color:#999;text-align:center;">
© {DateTime.Now.Year} Team AntiLag - Todos os direitos reservados.
</p>


</div>

</body>
</html>
""";


        await _resend.EmailSendAsync(email);

    }
}