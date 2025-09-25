using MongoDB.Driver;
using ConsumerService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsumerService.Services;

public class ConsumerMongoDbService : IConsumerMongoDbService
{
    private readonly IMongoCollection<ConsumedMessage> _consumedMessagesCollection;
    private readonly IMongoCollection<Student> _studentsCollection;
    private readonly IMongoDatabase _database;
    private readonly ILogger<ConsumerMongoDbService> _logger;

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public ConsumerMongoDbService(IConfiguration config, ILogger<ConsumerMongoDbService> logger)
    {
        _logger = logger;

        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        _database = client.GetDatabase(config["MongoDB:DatabaseName"]);

        _studentsCollection = _database.GetCollection<Student>("Students");
        _consumedMessagesCollection = _database.GetCollection<ConsumedMessage>("ConsumedMessages");

        _logger.LogInformation("Consumer MongoDB service initialized - Database: {DatabaseName}", config["MongoDB:DatabaseName"]);
    }

    public async Task<bool> SaveConsumedMessageAsync(ConsumedMessage message)
    {
        try
        {
            await _consumedMessagesCollection.InsertOneAsync(message);
            _logger.LogInformation("Consumed message saved to MongoDB with ID: {MessageId}", message.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save consumed message to MongoDB");
            return false;
        }
    }

    public async Task<(bool exists, bool success)> SaveOrUpdateStudentAsync(Student student)
    {
        var success = await UpdateStudentIfExistsAsync(student);
        return (success, success);
    }

 
    public async Task<bool> UpdateStudentIfExistsAsync(Student student)
    {
        try
        {
            var filter = Builders<Student>.Filter.Eq(s => s.RollNumber, student.RollNumber);
            var update = Builders<Student>.Update.Set(s => s.Name, student.Name);

            var result = await _studentsCollection.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("Student with RollNumber {RollNumber} does not exist.", student.RollNumber);
                return false;
            }

            _logger.LogInformation("Student {RollNumber} updated successfully.", student.RollNumber);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update student {RollNumber}", student.RollNumber);
            return false;
        }
    }

    public async Task<List<ConsumedMessage>> GetConsumedMessagesAsync(int page = 1, int pageSize = 50)
        => await _consumedMessagesCollection.Find(_ => true)
                                            .Skip((page - 1) * pageSize)
                                            .Limit(pageSize)
                                            .ToListAsync();

    public async Task<ConsumedMessage?> GetConsumedMessageByIdAsync(string id)
        => await _consumedMessagesCollection.Find(m => m.Id == id).FirstOrDefaultAsync();

    public async Task<object> GetConsumerStatsAsync()
    {
        var totalMessages = await _consumedMessagesCollection.CountDocumentsAsync(_ => true);
        return new { TotalMessages = totalMessages };
    }
}
