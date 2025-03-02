using MassTransit;
using WiredBrain.Messages;

namespace WiredBrain.Billing;

public class Program
{
    public static async Task Main()
    {
        // Configure MassTransit with RabbitMQ
        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host("rabbitmq", "/");
            
            cfg.ReceiveEndpoint("billing-service", e =>
            {
                e.Consumer<OrderPlacedConsumer>();
            });
        });

        await busControl.StartAsync();

        try
        {
            Console.WriteLine("Billing Service Started. Listening for orders...");
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
        Console.WriteLine($"Processing payment for order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
        return Task.CompletedTask;
    }
}
