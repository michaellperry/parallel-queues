using Polly;
using Polly.Timeout;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class TimeoutPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> Factory(ILogger logger, ResilienceConfig config)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            config.Timeout,
            TimeoutStrategy.Pessimistic,
            onTimeoutAsync: (context, timespan, task) =>
            {
                // Track timeout occurrence
                ResilienceMetrics.TrackTimeout();
                
                logger.LogWarning(
                    "Timeout after {TimeoutSeconds}s when calling payment service",
                    timespan.TotalSeconds);
                
                return Task.CompletedTask;
            });
    }
}
