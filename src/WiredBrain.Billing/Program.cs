using MassTransit;
using WiredBrain.Messages;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics;

namespace WiredBrain.Billing;

public class Program
{
    public static async Task Main()
    {
        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddPrometheusExporter()
            .Build();

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
            meterProvider.Dispose();
        }
    }
}

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    private static readonly Meter Meter = new Meter("WiredBrain.Billing");
    private static readonly Histogram<double> WaitTimeHistogram = Meter.CreateHistogram<double>("wait_time");
    private static readonly Counter<long> ProcessingTimeCounter = Meter.CreateCounter<long>("processing_time");

    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        WaitTimeHistogram.Record(waitTime);

        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Processing payment for order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
        stopwatch.Stop();
        ProcessingTimeCounter.Add(stopwatch.ElapsedMilliseconds);

        return Task.CompletedTask;
    }
}
