using ConsumerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConsumerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumerController : ControllerBase
{
    private readonly IConsumerMongoDbService _mongoService;
    private readonly ILogger<ConsumerController> _logger;

    public ConsumerController(IConsumerMongoDbService mongoService, ILogger<ConsumerController> logger)
    {
        _mongoService = mongoService;
        _logger = logger;
    }

    [HttpGet("consumed-messages")]
    public async Task<IActionResult> GetConsumedMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        => Ok(new { success = true, data = await _mongoService.GetConsumedMessagesAsync(page, pageSize), page, pageSize });

    [HttpGet("consumed-messages/{id}")]
    public async Task<IActionResult> GetConsumedMessage(string id)
    {
        var msg = await _mongoService.GetConsumedMessageByIdAsync(id);
        return msg == null ? NotFound(new { error = "Consumed message not found" }) : Ok(new { success = true, data = msg });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetConsumerStats()
    {
        var stats = await _mongoService.GetConsumerStatsAsync();
        return stats == null ? Problem("Failed to retrieve consumer statistics") : Ok(new { success = true, data = stats });
    }

    [HttpGet("/")]
    public IActionResult Root() => Ok("Consumer Service is running!");
}
