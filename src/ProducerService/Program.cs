using Microsoft.OpenApi.Models;
using ProducerService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

// Add logging
builder.Services.AddLogging();

// ✅ Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Producer Service API",
        Version = "v1",
        Description = "Sends messages to Kafka and stores them in MongoDB"
    });
});

builder.Services.AddControllers(); // <-- Register controllers

var app = builder.Build();

// ✅ Enable Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Producer Service API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseRouting();
app.MapControllers(); // <-- Map controller endpoints

// Health check endpoint (can stay here or move to its own controller)
app.MapGet("/health", () =>
    Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithName("HealthCheck");

app.Run();
