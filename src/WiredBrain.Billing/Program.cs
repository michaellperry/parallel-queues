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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMetricServer(); // Exposes /metrics endpoint
app.UseHttpMetrics();  // Collects HTTP request metrics

app.MapControllers();

app.Run();
