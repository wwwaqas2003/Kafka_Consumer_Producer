using ConsumerService;
using ConsumerService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConsumerServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Health endpoint
app.MapGet("/health", () =>
    Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow, service = "Consumer Service" }));

app.Run();
