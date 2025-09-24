using Confluent.Kafka;
using ProducerService.Models;

namespace ProducerService.Services
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<long, string> _producer;
        private readonly IMongoDbService _mongoService;
        private readonly string _topic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(string servers, string topic, IMongoDbService mongo, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            _mongoService = mongo;
            _topic = topic;

            _producer = new ProducerBuilder<long, string>(new ProducerConfig
            {
                BootstrapServers = servers,
                Acks = Acks.All,
                EnableIdempotence = true
            }).Build();
        }
        public long? LastGeneratedMessageId { get; private set; }

        public async Task<bool> ProduceMessageAsync(string message)
        {
            var result = await ProduceAndStoreMessageAsync(message);
            LastGeneratedMessageId = result.MessageId;
            return result.Success;
        }

        public async Task<ProduceResult> ProduceAndStoreMessageAsync(string message)
        {
            try
            {
                var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var result = await _producer.ProduceAsync(_topic, new Message<long, string> { Key = id, Value = message });
                _logger.LogInformation("Produced to {Topic} offset {Offset}", result.Topic, result.Offset);

                _ = _mongoService.SaveProducedMessageAsync(new ProducedMessage { Message = message, MessageId = id, CreatedAt = DateTime.UtcNow });

                return new ProduceResult { Success = true, MessageId = id, Timestamp = DateTime.UtcNow };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka production failed");
                return new ProduceResult { Success = false };
            }
        }

        public void Dispose() => _producer?.Dispose();
    }
}
