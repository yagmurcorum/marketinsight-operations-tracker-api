using MarketInsight.Api.DTOs.Observability;
using Microsoft.AspNetCore.Mvc;

namespace MarketInsight.Api.Controllers;

/// <summary>
/// Provides beginner-friendly observability summary information for the MarketInsight Operations Tracker API.
/// </summary>
[ApiController]
[Route("api/observability")]
public class ObservabilityController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservabilityController"/> class.
    /// </summary>
    /// <param name="environment">The current web host environment.</param>
    public ObservabilityController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Returns a summary of the application's basic observability features.
    /// </summary>
    /// <returns>Basic observability summary information.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ObservabilitySummaryResponse), StatusCodes.Status200OK)]
    public ActionResult<ObservabilitySummaryResponse> GetSummary()
    {
        var response = new ObservabilitySummaryResponse
        {
            ApplicationName = "MarketInsight Operations Tracker API",
            Environment = _environment.EnvironmentName,
            GeneratedAtUtc = DateTime.UtcNow,
            Summary = "This API exposes basic health, dependency status, structured logging and async processing observability information for development and demo purposes.",
            Endpoints = new List<string>
            {
                "GET /api/health",
                "GET /api/system/info",
                "GET /api/system/dependencies",
                "GET /api/observability/summary"
            },
            ObservabilityFeatures = new List<string>
            {
                "Basic health endpoint",
                "System information endpoint",
                "Dependency status endpoint",
                "SQLite dependency check",
                "Redis dependency check",
                "RabbitMQ dependency check",
                "External finance API dependency check",
                "Structured logging placeholders",
                "Redis cache hit and miss logs",
                "RabbitMQ publish and consume logs",
                "CorrelationId usage in async RabbitMQ flow",
                "Swagger XML Summary documentation"
            }
        };

        return Ok(response);
    }
}