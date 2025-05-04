using System.Diagnostics;
using MassTransit;
using WiredBrain.Messages;

namespace WiredBrain.Billing;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    // No static field needed as we'll use the delay from the message

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;

        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Processing payment for order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
        
        // Use the delay directly from the message
        // The Ordering service has already applied the randomization based on the coefficient of service variation
        Console.WriteLine($"Processing order with delay: {order.BillingProcessingDelayMs}ms (cs: {order.CoefficientOfServiceVariation})");
        
        // Use the delay from the message
        await Task.Delay(order.BillingProcessingDelayMs);
        
        stopwatch.Stop();
        
        // Convert milliseconds to seconds for the processing time metric
        double processingTimeSeconds = stopwatch.Elapsed.TotalSeconds;
        BillingMetrics.TrackProcessingTime(processingTimeSeconds);

        // Total wait time (W) includes both the time in queue and the processing time
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        BillingMetrics.TrackWaitTime(waitTime);
    }
}
