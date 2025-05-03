using System.Diagnostics;
using MassTransit;
using WiredBrain.Messages;

namespace WiredBrain.Billing;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    // Get the processing delay from environment variable or use default value (200ms)
    private static readonly int ProcessingDelayMs = GetProcessingDelayFromEnvironment();

    private static int GetProcessingDelayFromEnvironment()
    {
        string? delayStr = Environment.GetEnvironmentVariable("BILLING_PROCESSING_DELAY_MS");
        if (int.TryParse(delayStr, out int delay) && delay > 0)
        {
            return delay;
        }
        return 200; // Default delay if not specified or invalid
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;

        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Processing payment for order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
        
        // Use the configurable delay instead of hardcoded value
        await Task.Delay(ProcessingDelayMs);
        
        stopwatch.Stop();
        
        // Convert milliseconds to seconds for the processing time metric
        double processingTimeSeconds = stopwatch.Elapsed.TotalSeconds;
        BillingMetrics.TrackProcessingTime(processingTimeSeconds);

        // Total wait time (W) includes both the time in queue and the processing time
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        BillingMetrics.TrackWaitTime(waitTime);
    }
}
