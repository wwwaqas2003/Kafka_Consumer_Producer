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

        /* [HttpPost("produce")]
         public async Task<IActionResult> Produce([FromBody] tudents req)
         {
             if (string.IsNullOrWhiteSpace(req.Message))
                 return BadRequest(new { error = "Message cannot be empty" });

             var result = await _kafka.ProduceAndStoreMessageAsync(req.Message);
             if (!result.Success) return Problem("Failed to send message to Kafka");

             return Ok(new { success = true, req.Message, result.MessageId, result.Timestamp });
         }*/
        [HttpPost("produce-student")]
        public async Task<IActionResult> ProduceStudent([FromBody] Student student)
        {
            if (string.IsNullOrWhiteSpace(student.RollNumber) || string.IsNullOrWhiteSpace(student.Name))
                return BadRequest(new { error = "RollNumber and Name are required" });

            var jsonMessage = System.Text.Json.JsonSerializer.Serialize(student);
            var result = await _kafka.ProduceAndStoreMessageAsync(jsonMessage);

            if (!result.Success) return Problem("Failed to send message to Kafka");

            return Ok(new { success = true, student, result.MessageId, result.Timestamp });
        }


        [HttpGet("/")]
        public IActionResult Root() => Ok("Producer Service is running!");
    }
}
