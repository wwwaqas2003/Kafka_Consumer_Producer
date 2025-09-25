using ConsumerService.Models;
using MongoDB.Driver;

namespace ConsumerService.Services
{
    public interface IConsumerMongoDbService
    {
        Task<bool> SaveConsumedMessageAsync(ConsumedMessage message);
        Task<List<ConsumedMessage>> GetConsumedMessagesAsync(int page = 1, int pageSize = 50);
        Task<ConsumedMessage?> GetConsumedMessageByIdAsync(string id);
        Task<(bool exists, bool success)> SaveOrUpdateStudentAsync(Student student);

        Task<bool> UpdateStudentIfExistsAsync(Student student);
        Task<object> GetConsumerStatsAsync();
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
