using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Shared.DTOs;

namespace TeamX.API.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly ILicenseService _licenseService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public WebhookController(
        ILicenseService licenseService,
        IOrderService orderService,
        ICustomerService customerService,
        IEmailService emailService,
        ApplicationDbContext context)
    {
        _licenseService = licenseService;
        _orderService = orderService;
        _customerService = customerService;
        _context = context;
        _emailService = emailService;
    }


    [HttpPost("eremby")]
    public async Task<IActionResult> Eremby(
        [FromBody] ErembyWebhookRequest request)
    {
        if (request.PaymentStatus != "approved")
        {
            return Ok(new
            {
                success = false,
                message = "Pagamento não aprovado"
            });
        }


        // Verifica se esse pagamento já foi processado
        var existingOrder = await _context.Orders
            .FirstOrDefaultAsync(x =>
                x.TransactionId == request.TransactionId);


        if (existingOrder != null)
        {
            return Ok(new
            {
                success = true,
                message = "Webhook já processado"
            });
        }


        var product =
            await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == request.ProductId);


        var plan =
            await _context.Plans
            .FirstOrDefaultAsync(x =>
                x.PlanId == request.PlanId);


        if (product == null || plan == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Produto ou plano inválido"
            });
        }
        var customer =
            await _customerService.GetOrCreateAsync(
                request.CustomerEmail);



        var order = await _orderService.CreateAsync(
            customer.Id,
            product.Id,
            plan.PlanId,
            request.CustomerEmail,
            request.TransactionId);



        var license = await _licenseService.CreateLicenseAsync(
            new CreateLicenseRequest
            {
                CustomerId = customer.Id,
                ProductId = product.Id,
                PlanId = plan.PlanId
            });

        await _orderService.UpdateLicenseAsync(
            order.Id,
            license.Id);


        try
        {
            await _emailService.SendLicenseAsync(
                new LicenseEmailRequest
                {
                    CustomerEmail = request.CustomerEmail,
                    ProductName = product.Name,
                    LicenseKey = license.Key,
                    ExpirationDate = license.ExpiresAt,
                    DownloadLink = "https://teamantilag.com/download/teamx",
                    ActivationInstructions =
                    """
            1. Baixe o TeamX pelo link enviado.
            2. Instale e abra o aplicativo.
            3. Informe sua chave de licença.
            4. Clique em ativar.
            5. Aguarde a validação do servidor.
            """
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Erro ao enviar email: {ex.Message}");
        }


        return Ok(new
        {
            success = true,
            licenseKey = license.Key
        });
    }
}