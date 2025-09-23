using Microsoft.AspNetCore.Mvc;
using ProducerService.Models;
using ProducerService.Services;
namespace ProducerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProducerController : ControllerBase
    {
        private readonly IKafkaProducerService _kafkaService;
        // private readonly IMongoDbService _mongoService; // MongoDB removed
        private readonly ILogger<ProducerController> _logger;
        public ProducerController(
            IKafkaProducerService kafkaService,
            // IMongoDbService mongoService, // MongoDB removed
            ILogger<ProducerController> logger)
        {
            _kafkaService = kafkaService;
            // _mongoService = mongoService; // MongoDB removed
            _logger = logger;
        }
        [HttpPost("produce")]
        public async Task<IActionResult> Produce([FromBody] MessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message cannot be empty" });
            // Create the message object
            var producedMessage = new ProducedMessage
            {
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };
            // Save to MongoDB (commented out)
            // var mongoResult = await _mongoService.SaveProducedMessageAsync(producedMessage);
            // if (!mongoResult)
            // {
            //     _logger.LogError("Failed to save message to MongoDB");
            //     return Problem("Failed to save message to database");
            // }
            // Send to Kafka
            var kafkaResult = await _kafkaService.ProduceMessageAsync(request.Message);
            if (!kafkaResult)
            {
                _logger.LogError("Failed to send message to Kafka");
                return Problem("Failed to send message to Kafka");
            }

            // Get the messageId from Kafka producer service
            var messageId = _kafkaService.LastGeneratedMessageId;

            _logger.LogInformation("Message processed successfully: {Message}", request.Message);
            return Ok(new
            {
                success = true,
                message = "Message sent successfully",
                messageId = messageId,
                timestamp = producedMessage.CreatedAt
            });
        }
        [HttpGet("/")]
        public IActionResult Root()
        {
            return Ok("Producer Service is running!");
        }
    }
}