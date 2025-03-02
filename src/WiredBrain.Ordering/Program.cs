using MassTransit;
using WiredBrain.Messages;

namespace WiredBrain.Ordering;

public class Program
{
    public static async Task Main()
    {
        // Configure MassTransit with RabbitMQ
        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host("rabbitmq", "/");
        });

        await busControl.StartAsync();

        try
        {
            Console.WriteLine("Ordering Service Started. Publishing orders every second...");
            
            // Publish an order every second
            while (true)
            {
                var order = GenerateRandomOrder();
                await busControl.Publish<OrderPlaced>(order);
                Console.WriteLine($"Published order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
                await Task.Delay(1000);
            }
        }
        finally
        {
            await busControl.StopAsync();
        }
    }

    private static OrderPlaced GenerateRandomOrder()
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
}
