using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Shared.DTOs;

namespace TeamX.API.Controllers;

/// <summary>
/// Recebe e processa webhooks de pagamento da Eremby.
/// </summary>
[ApiController]
[Route("api/webhook")]
[Produces("application/json")]
public class WebhookController : ControllerBase
{
    private readonly ILicenseService _licenseService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<WebhookController> _logger;
    private readonly string _webhookSecret;

    // Mapeamento centralizado (fácil de manter)
    private static readonly Dictionary<string, (string ProductName, string PlanName)> VariantMap = new()
    {
        ["112169"] = ("TeamX Optimizer", "Basic"),
        ["112170"] = ("TeamX Optimizer", "Standard"),
        ["112171"] = ("TeamX Optimizer", "Professional")
    };

    public WebhookController(
        ILicenseService licenseService,
        IOrderService orderService,
        ICustomerService customerService,
        IEmailService emailService,
        ApplicationDbContext context,
        ILogger<WebhookController> logger,
        IConfiguration configuration)
    {
        _licenseService = licenseService;
        _orderService = orderService;
        _customerService = customerService;
        _emailService = emailService;
        _context = context;
        _logger = logger;

        _webhookSecret = configuration["Eremby:WebhookSecret"]
            ?? throw new InvalidOperationException("Eremby:WebhookSecret não configurado");
    }

    /// <summary>
    /// Endpoint de webhook da Eremby.
    /// </summary>
    [HttpPost("eremby")]
    [EnableRateLimiting("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Eremby(CancellationToken cancellationToken)
    {
        // 1. Validação de segurança
        if (!ValidateWebhookSecret())
        {
            _logger.LogWarning("Webhook não autorizado. IP: {Ip}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Unauthorized(new { success = false, message = "Webhook não autorizado" });
        }

        // 2. Leitura e deserialização
        var request = await DeserializeRequestAsync(cancellationToken);
        if (request is null)
            return BadRequest(new { success = false, message = "JSON inválido" });

        if (request.Order is null)
            return BadRequest(new { success = false, message = "Payload vazio ou inválido" });

        if (request.Items is null || request.Items.Count == 0)
            return BadRequest(new { success = false, message = "Nenhum item encontrado no pedido" });

        _logger.LogInformation(
            "Webhook Eremby | Event: {Event} | Status: {Status} | Transaction: {Transaction} | Email: {Email}",
            request.Event_Name,
            request.Order.Status,
            request.Order.Transaction_Id,
            request.Order.Customer?.Email);

        // 3. Só processa pagamentos aprovados
        if (request.Order.Status is not ("approved" or "paid" or "completed"))
        {
            return Ok(new { success = true, message = "Pagamento não aprovado — ignorado" });
        }

        // 4. Idempotência
        var alreadyExists = await _context.Orders
            .AnyAsync(x => x.TransactionId == request.Order.Transaction_Id, cancellationToken);

        if (alreadyExists)
        {
            _logger.LogInformation("Pedido já processado: {TransactionId}", request.Order.Transaction_Id);
            return Ok(new { success = true, message = "Venda já processada" });
        }

        // 5. Resolve produto e plano
        var variantId = request.Items[0].Variant_Id.ToString();

        if (!VariantMap.TryGetValue(variantId, out var mapping))
            return BadRequest(new { success = false, message = "Produto/Plano Eremby não configurado" });

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == mapping.ProductName, cancellationToken);

        if (product is null)
            return BadRequest(new { success = false, message = "Produto não encontrado" });

        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == mapping.PlanName, cancellationToken);

        if (plan is null)
            return BadRequest(new { success = false, message = "Plano não encontrado" });

        if (string.IsNullOrWhiteSpace(request.Order.Customer?.Email))
            return BadRequest(new { success = false, message = "E-mail do cliente ausente" });

        // 6. Processamento com transação
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await _customerService.GetOrCreateAsync(request.Order.Customer.Email);

            var order = await _orderService.CreateAsync(
                customer.Id,
                product.Id,
                plan.PlanId,
                request.Order.Customer.Email,
                request.Order.Transaction_Id);

            var license = await _licenseService.CreateLicenseAsync(new CreateLicenseRequest
            {
                CustomerId = customer.Id,
                ProductId = product.Id,
                PlanId = plan.PlanId
            });

            await _orderService.UpdateLicenseAsync(order.Id, license.Id);
            await transaction.CommitAsync(cancellationToken);

            // 7. Envio de e-mail (não deve quebrar o fluxo)
            await TrySendLicenseEmailAsync(request, product, license, order.Id);

            _logger.LogInformation(
                "Licença criada. Transaction: {TransactionId} | License: {MaskedKey}",
                request.Order.Transaction_Id,
                MaskKey(license.Key));

            // Nunca retornar a chave no response do webhook
            return Ok(new { success = true, message = "Licença criada" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex,
                "Erro ao processar webhook. Transaction: {TransactionId}",
                request.Order.Transaction_Id);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = "Erro interno ao processar o pedido" });
        }
    }

    #region Private Methods

    private bool ValidateWebhookSecret()
    {
        var received = Request.Headers["X-Webhook-Secret"].FirstOrDefault();

        if (string.IsNullOrEmpty(received))
            return false;

        var receivedBytes = Encoding.UTF8.GetBytes(received);
        var expectedBytes = Encoding.UTF8.GetBytes(_webhookSecret);

        // Proteção contra timing attack + verificação de tamanho
        return receivedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(receivedBytes, expectedBytes);
    }

    private async Task<ErembyWebhookRequest?> DeserializeRequestAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync(cancellationToken);

            return JsonSerializer.Deserialize<ErembyWebhookRequest>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao deserializar JSON do webhook Eremby");
            return null;
        }
    }

    private async Task TrySendLicenseEmailAsync(
        ErembyWebhookRequest request,
        TeamX.Core.Entities.Product product,
        TeamX.Core.Entities.License license,
        Guid orderId)
    {
        try
        {
            await _emailService.SendLicenseAsync(new LicenseEmailRequest
            {
                CustomerEmail = request.Order!.Customer!.Email,
                ProductName = product.Name,
                LicenseKey = license.Key,
                ExpirationDate = license.ExpiresAt,
                DownloadLink = "https://teamantilag.com/download/teamx",
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
            _logger.LogError(ex, "Erro ao enviar e-mail de licença. OrderId: {OrderId}", orderId);
        }
    }

    private static string MaskKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8)
            return "****";

        return $"{key[..3]}****{key[^4..]}";
    }

    #endregion
}