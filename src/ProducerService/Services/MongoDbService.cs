using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProducerService.Models;

namespace ProducerService.Services
{
    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<ProducedMessage> _producedMessagesCollection;
        private readonly ILogger<MongoDbService> _logger;

        public MongoDbService(string connectionString, string databaseName, ILogger<MongoDbService> logger)
        {
            _logger = logger;

            var settings = MongoClientSettings.FromConnectionString(connectionString); 
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
            settings.ConnectTimeout = TimeSpan.FromSeconds(30);

            var client = new MongoClient(settings);
            var database = client.GetDatabase(databaseName); 
            _producedMessagesCollection = database.GetCollection<ProducedMessage>("ProducedMessages");

            _logger.LogInformation("MongoDB service initialized with database: {DatabaseName}", databaseName);
        }


        public async Task<bool> SaveProducedMessageAsync(ProducedMessage message)
        {
            try
            {
                await _producedMessagesCollection.InsertOneAsync(message);
                _logger.LogInformation("Message saved to MongoDB with ID: {MessageId}", message.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save message to MongoDB");
                return false;
            }
        }
    }
}