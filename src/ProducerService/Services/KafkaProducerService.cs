using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProducerService.Services;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<long, string> _producer;
    private readonly string _topicName;
    private readonly string _bootstrapServers;
    private readonly ILogger<KafkaProducerService> _logger;
    private bool _isConnected = false;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _topicName = configuration["Kafka:TopicName"] ?? "messages";
        _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageTimeoutMs = 10000,
            RequestTimeoutMs = 5000,
            RetryBackoffMs = 1000,
            SocketTimeoutMs = 10000,
            MessageSendMaxRetries = 3
        };

        try
        {
            _producer = new ProducerBuilder<long, string>(config)
                //.SetErrorHandler((_, e) =>
                //{
                //    _logger.LogError("Kafka producer error: {Error}", e.Reason);
                //    _isConnected = false;
                //})
                //.SetLogHandler((_, logMessage) =>
                //{
                //    if (logMessage.Level <= SyslogLevel.Warning)
                //        _logger.LogWarning("Kafka log: {Message}", logMessage.Message);
                //})
                .Build();

            //_logger.LogInformation("Kafka producer initialized with bootstrap servers: {BootstrapServers}, topic: {TopicName}",
            //    _bootstrapServers, _topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Kafka producer");
            throw;
        }
    }

    public long? LastGeneratedMessageId { get; private set; }

    public async Task<bool> ProduceMessageAsync(string message)
    {
        if (_producer == null)
        {
            _logger.LogError("Kafka producer is not initialized");
            LastGeneratedMessageId = null;
            return false;
        }

        try
        {
            // Test connection if not already connected
            //if (!_isConnected)
            //{
            //    await TestConnectionAsync();
            //}

            var messageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            LastGeneratedMessageId = messageId;
            _logger.LogInformation("Generated MessageID: {MessageId}", messageId);

            var result = await _producer.ProduceAsync(
                _topicName,
                new Message<long, string>
                {
                    Key = messageId,
                    Value = message
                });

            _logger.LogInformation("Message produced to topic {Topic} with MessageID {MessageId} at offset {Offset}",
                result.Topic, messageId, result.Offset);

            _isConnected = true;
            return true;
        }
        catch (ProduceException<long, string> ex)
        {
            _logger.LogError(ex, "Failed to produce message to Kafka. Bootstrap servers: {BootstrapServers}, Topic: {Topic}",
                _bootstrapServers, _topicName);

            LastGeneratedMessageId = null;
            _isConnected = false;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error producing message to Kafka");
            LastGeneratedMessageId = null;
            _isConnected = false;
            return false;
        }
    }

    //private async Task TestConnectionAsync()
    //{
    //    try
    //    {
    //        var testMessageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    //
    //        var result = await _producer.ProduceAsync(
    //            _topicName,
    //            new Message<long, string>
    //            {
    //                Key = testMessageId,
    //                Value = "connection_test"
    //            });
    //
    //        _logger.LogInformation("Successfully connected to Kafka. Topic: {Topic}, Test MessageID: {MessageId}, Offset: {Offset}",
    //            result.Topic, testMessageId, result.Offset);
    //
    //        _isConnected = true;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogWarning(ex, "Failed to test Kafka connection");
    //        _isConnected = false;
    //    }
    //}

    public void Dispose()
    {
        try
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            //_logger.LogInformation("Kafka producer disposed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing Kafka producer");
        }
    }
}