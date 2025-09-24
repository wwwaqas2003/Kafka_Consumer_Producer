using Microsoft.AspNetCore.Mvc;
using ProducerService.Models;
using ProducerService.Services;

namespace ProducerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProducerController : ControllerBase
    {
        private readonly IKafkaProducerService _kafka;
        private readonly ILogger<ProducerController> _logger;

        public ProducerController(IKafkaProducerService kafka, ILogger<ProducerController> logger)
        {
            _kafka = kafka;
            _logger = logger;
        }

        [HttpPost("produce")]
        public async Task<IActionResult> Produce([FromBody] MessageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { error = "Message cannot be empty" });

            var result = await _kafka.ProduceAndStoreMessageAsync(req.Message);
            if (!result.Success) return Problem("Failed to send message to Kafka");

            return Ok(new { success = true, req.Message, result.MessageId, result.Timestamp });
        }

        [HttpGet("/")]
        public IActionResult Root() => Ok("Producer Service is running!");
    }
}
