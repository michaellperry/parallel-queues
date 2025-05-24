using MassTransit;
using Prometheus;
using WiredBrain.Billing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/");
            
        cfg.ReceiveEndpoint("billing-service", e =>
        {
            e.Consumer<OrderPlacedConsumer>();
            
            // Use the default MassTransit concurrency settings based on CPU count
            int concurrentMessages = e.ConcurrentMessageLimit ?? e.PrefetchCount;
            BillingMetrics.TrackNumProcessors(concurrentMessages); // Track the number of concurrent messages that can be processed
            
            Console.WriteLine($"Billing service configured with {concurrentMessages} concurrent processors");
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WiredBrain Billing API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WiredBrain Billing API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMetricServer(); // Exposes /metrics endpoint
app.UseHttpMetrics();  // Collects HTTP request metrics

app.Run();
