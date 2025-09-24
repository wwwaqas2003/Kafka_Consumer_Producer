using ProducerService.Models;

namespace ProducerService.Services
{
    public interface IKafkaProducerService
    {
        long? LastGeneratedMessageId { get; }
        Task<bool> ProduceMessageAsync(string message);
        Task<ProduceResult> ProduceAndStoreMessageAsync(string message);
    }
}