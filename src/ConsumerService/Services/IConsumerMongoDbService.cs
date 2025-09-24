using ConsumerService.Models;

namespace ConsumerService.Services
{
    public interface IConsumerMongoDbService
    {
        Task<bool> SaveConsumedMessageAsync(ConsumedMessage message);
        Task<List<ConsumedMessage>> GetConsumedMessagesAsync(int page = 1, int pageSize = 50);
        Task<ConsumedMessage?> GetConsumedMessageByIdAsync(string id);
        Task<object> GetConsumerStatsAsync();
    }
}
