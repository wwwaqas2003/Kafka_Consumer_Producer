using Confluent.Kafka;
using ConsumerService.Services;

namespace ConsumerService;

public static class ServiceCollectionExtensions
{
    public static void AddConsumerServices(this IServiceCollection services, IConfiguration config)
    {
        // MongoDB
        services.AddSingleton<IConsumerMongoDbService>(sp =>
            new ConsumerMongoDbService(
                config["MongoDB:ConnectionString"],
                config["MongoDB:DatabaseName"],
                sp.GetRequiredService<ILogger<ConsumerMongoDbService>>()));

        // Kafka Consumer
        services.AddSingleton<IConsumer<Ignore, string>>(_ =>
            new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = config["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build());

        services.AddHostedService<KafkaConsumerService>(sp =>
            new KafkaConsumerService(
                sp.GetRequiredService<IConsumer<Ignore, string>>(),
                sp.GetRequiredService<IConsumerMongoDbService>(),
                sp.GetRequiredService<ILogger<KafkaConsumerService>>(),
                config["Kafka:TopicName"]));
    }
}
