using Microsoft.AspNetCore.Mvc;
using TeamX.Shared.DTOs.Health;
using TeamX.Shared.DTOs.Responses;

namespace TeamX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var response = new HealthResponse();

        return Ok(ApiResponse<HealthResponse>.Ok(response));
    }
}