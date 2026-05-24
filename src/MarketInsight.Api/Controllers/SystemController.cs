using Microsoft.AspNetCore.Mvc;

namespace MarketInsight.Api.Controllers;

/// <summary>
/// Provides basic system information about the MarketInsight Operations Tracker API.
/// </summary>
[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public SystemController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Returns basic application name, version, and environment information.
    /// </summary>
    /// <returns>Basic system information about the API.</returns>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSystemInfo()
    {
        var response = new
        {
            applicationName = "MarketInsight Operations Tracker API",
            version = "1.0.0",
            environment = _environment.EnvironmentName
        };

        return Ok(response);
    }
}
