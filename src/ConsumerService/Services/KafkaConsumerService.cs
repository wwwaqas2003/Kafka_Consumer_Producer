using Confluent.Kafka;
using ConsumerService.Models;

namespace ConsumerService.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IConsumerMongoDbService _mongo;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _topic;

    public KafkaConsumerService(
        IConsumer<Ignore, string> consumer,
        IConsumerMongoDbService mongo,
        ILogger<KafkaConsumerService> logger,
        string topic)
    {
        _consumer = consumer;
        _mongo = mongo;
        _logger = logger;
        _topic = topic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Subscribed to {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    if (result?.Message?.Value == null)
                        continue;

                    _logger.LogInformation("Message at offset {Offset}", result.Offset);

           
                    var student = System.Text.Json.JsonSerializer.Deserialize<Student>(result.Message.Value);
                    if (student == null)
                        continue;

                    // ye rl
                    if (string.IsNullOrEmpty(student.RollNumber))
                        student.RollNumber = result.Message.Key.ToString();

                    
                    bool exists = await _mongo.UpdateStudentIfExistsAsync(student);

                    if (exists)
                    {
                        _logger.LogInformation("Student {RollNumber} exists. Name updated to {Name}", student.RollNumber, student.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Student {RollNumber} does not exist. Cannot update name.", student.RollNumber);
                    }

                    _consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Consume error");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        finally
        {
            _consumer.Unsubscribe();
            _consumer.Close();
            _logger.LogInformation("Consumer closed");
        }
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}
