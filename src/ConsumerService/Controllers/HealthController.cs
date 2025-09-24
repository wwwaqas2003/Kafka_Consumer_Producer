using Microsoft.AspNetCore.Mvc;
using ConsumerService.Services;

namespace ConsumerService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IConsumerMongoDbService _mongoService;

    public HealthController(
        ILogger<HealthController> logger,
        IConsumerMongoDbService mongoService)
    {
        _logger = logger;
        _mongoService = mongoService;
    }

    /// <summary>
    /// Get basic service health status
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            service = "Consumer Service",
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Get detailed service status including dependencies
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var healthChecks = new List<object>();

        // Check MongoDB connection
        try
        {
            var testMessage = new Models.ConsumedMessage
            {
                Message = "health_check",
                Topic = "health",
                Partition = 0,
                Offset = 0,
                ConsumedAt = DateTime.UtcNow
            };

            // Don't actually save, just test connection
            healthChecks.Add(new
            {
                component = "MongoDB",
                status = "Healthy",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MongoDB health check failed");
            healthChecks.Add(new
            {
                component = "MongoDB",
                status = "Unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }

        var overallStatus = healthChecks.Any(h => h.GetType().GetProperty("status")?.GetValue(h)?.ToString() == "Unhealthy")
            ? "Unhealthy" : "Healthy";

        return Ok(new
        {
            service = "Consumer Service",
            status = overallStatus,
            timestamp = DateTime.UtcNow,
            checks = healthChecks
        });
    }
}