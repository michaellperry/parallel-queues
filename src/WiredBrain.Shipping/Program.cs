using MassTransit;
using WiredBrain.Messages;

namespace WiredBrain.Shipping;

public class Program
{
    public static async Task Main()
    {
        // Configure MassTransit with RabbitMQ
        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host("rabbitmq", "/");
            
            cfg.ReceiveEndpoint("shipping-service", e =>
            {
                e.Consumer<OrderPlacedConsumer>();
            });
        });

        await busControl.StartAsync();

        try
        {
            Console.WriteLine("Shipping Service Started. Listening for orders...");
            await Task.Delay(Timeout.InfiniteTimeSpan);
        }
        finally
        {
            await busControl.StopAsync();
        }
    }
}

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;
        Console.WriteLine($"Preparing shipment for order: {order.OrderId} for {order.CustomerName}");
        return Task.CompletedTask;
    }
}
