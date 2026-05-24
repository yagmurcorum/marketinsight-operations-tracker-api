using Microsoft.AspNetCore.Mvc;

namespace MarketInsight.Api.Controllers;

/// <summary>
/// Provides basic health information about the MarketInsight Operations Tracker API.
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Checks whether the API is running.
    /// </summary>
    /// <returns>Basic health information about the API.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        var response = new
        {
            status = "Healthy",
            application = "MarketInsight Operations Tracker API",
            timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}
