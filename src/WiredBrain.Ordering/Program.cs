using MassTransit;
using Prometheus;
using WiredBrain.Messages;
using WiredBrain.Ordering;

var builder = WebApplication.CreateBuilder(args);

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/");
    });
});

var app = builder.Build();

// Configure metrics endpoint
app.UseMetricServer();

// Start the order publishing background service
Task.Run(async () => await PublishOrdersAsync());

app.Run();

async Task PublishOrdersAsync()
{
    // Get the bus from the service provider
    var busControl = app.Services.GetRequiredService<IBus>();

    try
    {
        Console.WriteLine("Ordering Service Started. Publishing orders...");
        
        // Get the configurable arrival rate from environment variable or use default
        var orderArrivalRateMs = Environment.GetEnvironmentVariable("ORDER_ARRIVAL_RATE_MS");
        var delayMs = string.IsNullOrEmpty(orderArrivalRateMs) ? 1000 : int.Parse(orderArrivalRateMs);
        
        Console.WriteLine($"Order arrival rate set to: {delayMs}ms between orders");
        
        // Publish orders at the configured rate
        while (true)
        {
            var order = GenerateRandomOrder();
            await busControl.Publish<OrderPlaced>(order);
            
            // Track the order placement in metrics
            OrderingMetrics.TrackOrderPlaced();
            
            Console.WriteLine($"Published order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
            
            await Task.Delay(delayMs);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error publishing orders: {ex.Message}");
    }
}

OrderPlaced GenerateRandomOrder()
{
    var random = new Random();
    var customers = new[] { "John", "Jane", "Bob", "Alice", "Charlie" };
    
    return new OrderPlaced
    {
        OrderId = Guid.NewGuid(),
        CustomerName = customers[random.Next(customers.Length)],
        OrderDate = DateTime.UtcNow,
        Amount = (decimal)Math.Round(random.NextSingle() * 100, 2)
    };
}
