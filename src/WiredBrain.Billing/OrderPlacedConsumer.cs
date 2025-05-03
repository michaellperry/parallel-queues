using System.Diagnostics;
using MassTransit;
using WiredBrain.Messages;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    // private static readonly Meter Meter = new Meter("WiredBrain.Billing");
    // private static readonly Histogram<double> WaitTimeHistogram = Meter.CreateHistogram<double>("wait_time");
    // private static readonly Counter<long> ProcessingTimeCounter = Meter.CreateCounter<long>("processing_time");

    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        // WaitTimeHistogram.Record(waitTime);

        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Processing payment for order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
        stopwatch.Stop();
        // ProcessingTimeCounter.Add(stopwatch.ElapsedMilliseconds);

        return Task.CompletedTask;
    }
}
