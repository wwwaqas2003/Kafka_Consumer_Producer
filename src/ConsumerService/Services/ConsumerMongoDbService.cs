using MongoDB.Driver;
using ConsumerService.Models;

namespace ConsumerService.Services;

public class ConsumerMongoDbService : IConsumerMongoDbService
{
    private readonly IMongoCollection<ConsumedMessage> _consumedMessagesCollection;
    private readonly ILogger<ConsumerMongoDbService> _logger;

    public ConsumerMongoDbService(
        string connectionString,
        string databaseName,
        ILogger<ConsumerMongoDbService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
        settings.ConnectTimeout = TimeSpan.FromSeconds(30);

        var client = new MongoClient(settings);
        var database = client.GetDatabase(databaseName);
        _consumedMessagesCollection = database.GetCollection<ConsumedMessage>("ConsumedMessages");

        _logger.LogInformation("Consumer MongoDB service initialized - Database: {DatabaseName}", databaseName);
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

    public async Task<List<ConsumedMessage>> GetConsumedMessagesAsync(int page = 1, int pageSize = 50)
    {
        return await _consumedMessagesCollection
            .Find(_ => true)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<ConsumedMessage?> GetConsumedMessageByIdAsync(string id)
    {
        return await _consumedMessagesCollection
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<object> GetConsumerStatsAsync()
    {
        var totalMessages = await _consumedMessagesCollection.CountDocumentsAsync(_ => true);
        return new { TotalMessages = totalMessages };
    }
}
