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
        _emailService = emailService;
        _context = context;
    }


    [HttpPost("eremby")]
    public async Task<IActionResult> Eremby(
        [FromBody] ErembyWebhookRequest request)
    {

        // ================================
        // Verifica pagamento
        // ================================
        Console.WriteLine("===== EREEMBY =====");
        Console.WriteLine($"EVENT: {request.Event}");
        Console.WriteLine($"STATUS: {request.Data.Status}");
        Console.WriteLine($"TRANSACTION: {request.Data.Id}");
        Console.WriteLine($"PRODUCT: {request.Data.Product.Id}");
        Console.WriteLine($"EMAIL: {request.Data.Customer.Email}");
        if (
            request.Data.Status != "approved" &&
            request.Data.Status != "paid" &&
            request.Data.Status != "completed"
        )
        {
            return Ok(new
            {
                success = false,
                message = "Pagamento não aprovado",
                status = request.Data.Status
            });
        }



        // ================================
        // Evita duplicação
        // ================================

        var existingOrder =
            await _context.Orders
            .FirstOrDefaultAsync(x =>
                x.TransactionId == request.Data.Id);


        if (existingOrder != null)
        {
            return Ok(new
            {
                success = true,
                message = "Venda já processada"
            });
        }



        // ================================
        // Localizar produto TeamX
        // ================================

        var product =
            await GetTeamXProduct(
                request.Data.Product.Id);



        if (product == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Produto Ereemby não configurado"
            });
        }



        // ================================
        // Localizar plano
        // ================================

        var plan =
            await GetTeamXPlan(
                request.Data.Product.Id);



        if (plan == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Plano não encontrado"
            });
        }




        // ================================
        // Criar cliente
        // ================================

        var customer =
            await _customerService.GetOrCreateAsync(
                request.Data.Customer.Email);




        // ================================
        // Criar pedido
        // ================================

        var order =
            await _orderService.CreateAsync(
                customer.Id,
                product.Id,
                plan.PlanId,
                request.Data.Customer.Email,
                request.Data.Id);




        // ================================
        // Criar licença
        // ================================

        var license =
            await _licenseService.CreateLicenseAsync(
                new CreateLicenseRequest
                {
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    PlanId = plan.PlanId
                });



        // ================================
        // Vincular licença ao pedido
        // ================================

        await _orderService.UpdateLicenseAsync(
            order.Id,
            license.Id);




        // ================================
        // Enviar email
        // ================================

        try
        {
            await _emailService.SendLicenseAsync(
                new LicenseEmailRequest
                {
                    CustomerEmail =
                        request.Data.Customer.Email,

                    ProductName =
                        product.Name,

                    LicenseKey =
                        license.Key,

                    ExpirationDate =
                        license.ExpiresAt,

                    DownloadLink =
                        "https://teamantilag.com/download/teamx",

                    ActivationInstructions =
                    """
                    1. Baixe o TeamX.
                    2. Instale o aplicativo.
                    3. Abra o TeamX.
                    4. Informe sua chave.
                    5. Clique em ativar.
                    """
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Erro email: {ex.Message}");
        }




        return Ok(new
        {
            success = true,
            message = "Licença criada",
            licenseKey = license.Key
        });
    }

    private async Task<TeamX.Core.Entities.Product?> GetTeamXProduct(
        string ereembyProductId)
    {
        return ereembyProductId switch
        {
            "112169" =>
                await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Name == "TeamX Optimizer"),

            "112170" =>
                await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Name == "TeamX Optimizer"),

            "112171" =>
                await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Name == "TeamX Optimizer"),

            _ => null
        };
    }

    private async Task<TeamX.Core.Entities.Plan?> GetTeamXPlan(
    string ereembyProductId)
    {
        return ereembyProductId switch
        {
            "112169" =>
                await _context.Plans
                .FirstOrDefaultAsync(
                    x => x.Name == "Basic"),


            "112170" =>
                await _context.Plans
                .FirstOrDefaultAsync(
                    x => x.Name == "Standard"),


            "112171" =>
                await _context.Plans
                .FirstOrDefaultAsync(
                    x => x.Name == "Professional"),


            _ => null
        };
    }
}