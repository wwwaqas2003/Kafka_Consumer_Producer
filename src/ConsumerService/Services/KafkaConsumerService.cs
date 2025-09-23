using Confluent.Kafka;

namespace ConsumerService.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly string _topicName;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;
        _topicName = configuration["Kafka:TopicName"] ?? "messages";

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = configuration["Kafka:GroupId"] ?? "consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _logger.LogInformation("Kafka consumer initialized with topic: {TopicName}, GroupId: {GroupId}", 
            _topicName, config.GroupId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topicName);
        _logger.LogInformation("Consumer subscribed to topic: {TopicName}", _topicName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    
                    if (consumeResult?.Message?.Value != null)
                    {
                        _logger.LogInformation("Consumed message: {Message} from topic: {Topic}, partition: {Partition}, offset: {Offset}",
                            consumeResult.Message.Value,
                            consumeResult.Topic,
                            consumeResult.Partition,
                            consumeResult.Offset);

                      
                        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Consumed: {consumeResult.Message.Value}");

                     
                        _consumer.StoreOffset(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                    
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer operation was cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in consumer loop");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Kafka consumer");
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer closed");
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}