using Confluent.Kafka;
using ProducerService.Services;

namespace ProducerService
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            // Mongo
            services.AddSingleton<IMongoDbService>(sp =>
                new MongoDbService(
                    config["MongoDB:ConnectionString"],
                    config["MongoDB:DatabaseName"],
                    sp.GetRequiredService<ILogger<MongoDbService>>()));

            // Kafka
            services.AddScoped<IKafkaProducerService>(sp =>
                new KafkaProducerService(
                    config["Kafka:BootstrapServers"],
                    config["Kafka:TopicName"],
                    sp.GetRequiredService<IMongoDbService>(),
                    sp.GetRequiredService<ILogger<KafkaProducerService>>()));
        }
    }
}
