using MassTransit;
using WiredBrain.Messages;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics;

namespace WiredBrain.Ordering;

public class Program
{
    private static DateTime _lastOrderTime = DateTime.MinValue;
    private static double _totalInterArrivalTime = 0;
    private static int _orderCount = 0;

    public static async Task Main()
    {
        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddPrometheusExporter()
            .Build();

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

                // Track order arrival times and calculate average arrival rate
                if (_lastOrderTime != DateTime.MinValue)
                {
                    var interArrivalTime = (DateTime.UtcNow - _lastOrderTime).TotalSeconds;
                    _totalInterArrivalTime += interArrivalTime;
                    _orderCount++;
                }
                _lastOrderTime = DateTime.UtcNow;

                var averageArrivalRate = _orderCount / _totalInterArrivalTime;
                Console.WriteLine($"Average Arrival Rate: {averageArrivalRate} orders/second");

                await Task.Delay(1000);
            }
        }
        finally
        {
            await busControl.StopAsync();
            meterProvider.Dispose();
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
