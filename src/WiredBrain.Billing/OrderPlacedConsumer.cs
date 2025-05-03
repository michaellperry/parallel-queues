using System.Diagnostics;
using MassTransit;
using WiredBrain.Messages;

namespace WiredBrain.Billing;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;

        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Processing payment for order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
        await Task.Delay(200); // Simulate payment processing delay (τ)
        stopwatch.Stop();
        BillingMetrics.TrackProcessingTime(stopwatch.ElapsedMilliseconds);

        // Total wait time (W) includes both the time in queue and the processing time
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        BillingMetrics.TrackWaitTime(waitTime);
    }
}
