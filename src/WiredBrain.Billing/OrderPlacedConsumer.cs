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
        
        // Apply Kingman's formula to adjust the processing time based on the coefficient of service variation
        int processingDelay = order.BillingProcessingDelayMs;
        
        // Only apply variation if the coefficient is greater than 0
        if (order.CoefficientOfServiceVariation > 0)
        {
            var random = new Random();
            
            // Generate a random factor based on the coefficient of variation
            // Using a normal distribution centered around 1.0
            var standardDeviation = order.CoefficientOfServiceVariation;
            
            // Box-Muller transform for normal distribution
            var u1 = random.NextDouble();
            var u2 = random.NextDouble();
            var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            
            // Apply the variation factor (centered around 1.0)
            var variationFactor = 1.0 + (z * standardDeviation);
            
            // Ensure we don't get negative processing times
            variationFactor = Math.Max(0.1, variationFactor);
            
            processingDelay = (int)(order.BillingProcessingDelayMs * variationFactor);
        }
        
        Console.WriteLine($"Processing order with delay: {processingDelay}ms (base: {order.BillingProcessingDelayMs}ms, cs: {order.CoefficientOfServiceVariation})");
        
        // Use the adjusted delay
        await Task.Delay(processingDelay);
        
        stopwatch.Stop();
        
        // Convert milliseconds to seconds for the processing time metric
        double processingTimeSeconds = stopwatch.Elapsed.TotalSeconds;
        BillingMetrics.TrackProcessingTime(processingTimeSeconds);

        // Total wait time (W) includes both the time in queue and the processing time
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        BillingMetrics.TrackWaitTime(waitTime);
    }
}
