using System.Reflection;
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
    /// Verifica o status de saúde da API (liveness simples).
    /// Para readiness com Postgres use GET /health (MapHealthChecks).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? "1.0.0";

        var response = new HealthResponse
        {
            Name = "TeamX API",
            Version = version,
            Status = "Healthy",
            ServerTime = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<HealthResponse>.Ok(response));
    }
}