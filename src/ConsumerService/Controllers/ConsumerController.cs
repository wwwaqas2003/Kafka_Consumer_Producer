using MongoDB.Driver;
using ConsumerService.Models;
using ConsumerService.Services;
using Microsoft.AspNetCore.Mvc;


namespace ConsumerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumerController : ControllerBase
{
    private readonly IConsumerMongoDbService _mongo;
    private readonly ILogger<ConsumerController> _logger;

    public ConsumerController(IConsumerMongoDbService mongoService, ILogger<ConsumerController> logger)
    {
        _mongo = mongoService;
        _logger = logger;
    }

    [HttpGet("students/{rollNumber}")]
    public async Task<IActionResult> GetStudent(string rollNumber)
    {
        var studentCollection = _mongo.GetCollection<Student>("Students");
        var student = await studentCollection.Find(s => s.RollNumber == rollNumber).FirstOrDefaultAsync();
        if (student == null) return NotFound(new { error = "Student not found" });
        return Ok(new { success = true, data = student });
    }
}
