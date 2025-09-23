using ConsumerService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add the Kafka consumer service
builder.Services.AddHostedService<KafkaConsumerService>();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Consumer Service starting up...");

await host.RunAsync();