using ProducerService.Models;

namespace ProducerService.Services;

public interface IMongoDbService
{
    Task<bool> SaveProducedMessageAsync(ProducedMessage message);
}