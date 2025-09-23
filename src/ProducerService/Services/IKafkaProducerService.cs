namespace ProducerService.Services;

public interface IKafkaProducerService
{
    Task<bool> ProduceMessageAsync(string message);
    long? LastGeneratedMessageId { get; }
}