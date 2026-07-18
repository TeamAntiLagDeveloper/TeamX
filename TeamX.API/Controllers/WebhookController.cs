using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
    public async Task<IActionResult> Eremby()
    {

        // ==================================
        // Captura JSON real da Ereemby
        // ==================================

        using var reader = new StreamReader(Request.Body);

        var json = await reader.ReadToEndAsync();


        Console.WriteLine("===== JSON BRUTO EREEMBY =====");
        Console.WriteLine(json);



        ErembyWebhookRequest? request;


        try
        {
            request = JsonSerializer.Deserialize<ErembyWebhookRequest>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return BadRequest(new
            {
                success = false,
                message = "JSON inválido"
            });
        }



        if (request == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Payload vazio"
            });
        }



        Console.WriteLine("===== DADOS PROCESSADOS =====");
        Console.WriteLine($"EVENT: {request.Event_Name}");
        Console.WriteLine($"STATUS: {request.Order.Status}");
        Console.WriteLine($"TRANSACTION: {request.Order.Transaction_Id}");
        Console.WriteLine($"PRODUCT: {request.Items[0].Variant_Id}");
        Console.WriteLine($"EMAIL: {request.Order.Customer.Email}");



        // ==================================
        // Verifica pagamento
        // ==================================

        if (
            request.Order.Status != "approved" &&
            request.Order.Status != "paid" &&
            request.Order.Status != "completed"
        )
        {
            return Ok(new
            {
                success = false,
                message = "Pagamento não aprovado"
            });
        }



        // ==================================
        // Evitar duplicação
        // ==================================

        var existingOrder =
            await _context.Orders
            .FirstOrDefaultAsync(x =>
                x.TransactionId == request.Order.Transaction_Id);



        if (existingOrder != null)
        {
            return Ok(new
            {
                success = true,
                message = "Venda já processada"
            });
        }



        // ==================================
        // Produto TeamX
        // ==================================

        var product =
            await GetTeamXProduct(
                request.Items[0].Variant_Id.ToString());



        if (product == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Produto Ereemby não configurado"
            });
        }



        // ==================================
        // Plano
        // ==================================

        var plan =
            await GetTeamXPlan(
                request.Items[0].Variant_Id.ToString());



        if (plan == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Plano não encontrado"
            });
        }



        // ==================================
        // Cliente
        // ==================================

        var customer =
            await _customerService.GetOrCreateAsync(
                request.Order.Customer.Email);



        // ==================================
        // Pedido
        // ==================================

        var order =
            await _orderService.CreateAsync(
                customer.Id,
                product.Id,
                plan.PlanId,
                request.Order.Customer.Email,
                request.Order.Transaction_Id);



        // ==================================
        // Licença
        // ==================================

        var license =
            await _licenseService.CreateLicenseAsync(
                new CreateLicenseRequest
                {
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    PlanId = plan.PlanId
                });



        await _orderService.UpdateLicenseAsync(
            order.Id,
            license.Id);



        // ==================================
        // Email
        // ==================================

        try
        {
            await _emailService.SendLicenseAsync(
                new LicenseEmailRequest
                {
                    CustomerEmail = request.Order.Customer.Email,

                    ProductName = product.Name,

                    LicenseKey = license.Key,

                    ExpirationDate = license.ExpiresAt,

                    DownloadLink =
                    "https://teamantilag.com/download/teamx",

                    ActivationInstructions =
                    """
                    1. Baixe o TeamX.
                    2. Instale.
                    3. Abra o aplicativo.
                    4. Informe sua chave.
                    5. Ative.
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
        string id)
    {
        return id switch
        {
            "112169" or "112170" or "112171" =>
                await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Name == "TeamX Optimizer"),

            _ => null
        };
    }



    private async Task<TeamX.Core.Entities.Plan?> GetTeamXPlan(
        string id)
    {
        return id switch
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