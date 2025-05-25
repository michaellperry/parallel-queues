using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class CircuitBreakerPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> Factory(ILogger logger, ResilienceConfig config)
    {
        logger.LogWarning("Configuring circuit breaker policy with " +
                          "{ExceptionsAllowedBeforeBreaking} exceptions allowed before breaking, " +
                          "{DurationOfBreak} seconds duration of break",
            config.ExceptionsAllowedBeforeBreaking,
            config.DurationOfBreak.TotalSeconds);
            
        var policy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: config.ExceptionsAllowedBeforeBreaking,
                durationOfBreak: config.DurationOfBreak,
                onBreak: (exception, duration) =>
                {
                    logger.LogWarning(
                        "Circuit breaker opened due to {ExceptionType}: {ExceptionMessage}. " +
                        "Will remain open for {DurationSeconds} seconds",
                        exception?.GetType().Name ?? "HttpError",
                        exception?.Message,
                        duration.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker closed. Payment service calls will resume");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open. Testing payment service availability");
                });
                
        return policy.AsAsyncPolicy<HttpResponseMessage>();
    }
}
