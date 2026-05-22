using Microsoft.AspNetCore.Mvc;

namespace MarketInsight.Api.Controllers;

[ApiController]
[Route("api/health")]
public class  HealthController:ControllerBase
{
    [HttpGet]
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
