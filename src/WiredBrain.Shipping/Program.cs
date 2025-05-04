using System.Diagnostics;
using MassTransit;
using Prometheus;
using WiredBrain.Messages;
using WiredBrain.Shipping;

var builder = WebApplication.CreateBuilder(args);

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/");

        cfg.ReceiveEndpoint("shipping-service", e =>
        {
            e.ConfigureConsumer<OrderPlacedConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();

// Add Prometheus metrics middleware
app.UseMetricServer();
app.UseHttpMetrics();

app.MapGet("/", () => "WiredBrain Shipping Service");

app.Run();

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    // Fixed processing delay of 300ms
    private static readonly int ProcessingDelayMs = 300;

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;

        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Preparing shipment for order: {order.OrderId} for {order.CustomerName}");
        
        // Use the fixed processing delay
        await Task.Delay(ProcessingDelayMs);
        
        stopwatch.Stop();
        
        // Convert milliseconds to seconds for the processing time metric
        double processingTimeSeconds = stopwatch.Elapsed.TotalSeconds;
        ShippingMetrics.TrackProcessingTime(processingTimeSeconds);

        // Total wait time (W) includes both the time in queue and the processing time
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        ShippingMetrics.TrackWaitTime(waitTime);
        
        Console.WriteLine($"Shipment prepared for order: {order.OrderId}");
    }
}
