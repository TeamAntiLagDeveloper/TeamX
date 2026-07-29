using Microsoft.AspNetCore.Mvc;
using TeamX.Shared.DTOs.Health;
using TeamX.Shared.DTOs.Responses;

namespace TeamX.API.Controllers;

/// <summary>
/// Endpoint de health check da API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Verifica o status de saúde da API.
    /// </summary>
    /// <returns>Status de saúde da aplicação.</returns>
    /// <response code="200">API está saudável.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var response = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
            // Adicione outras propriedades relevantes do seu DTO se existirem
            // Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            // Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        };

        return Ok(ApiResponse<HealthResponse>.Ok(response));
    }
}