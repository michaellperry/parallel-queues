using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class CircuitBreakerPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> Factory(ILogger logger, ResilienceConfig config)
    {
        logger.LogWarning("Configuring circuit breaker policy with {SamplingDuration} seconds, " +
                          "{ExceptionsAllowedBeforeBreaking} exceptions allowed before breaking, " +
                          "{DurationOfBreak} seconds duration of break",
            config.SamplingDuration.TotalSeconds,
            config.ExceptionsAllowedBeforeBreaking,
            config.DurationOfBreak.TotalSeconds);
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .Or<TaskCanceledException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5, // 50% failure rate
                samplingDuration: config.SamplingDuration,
                minimumThroughput: config.ExceptionsAllowedBeforeBreaking,
                durationOfBreak: config.DurationOfBreak,
                onBreak: (result, duration) =>
                {
                    var exception = result.Exception;
                    logger.LogWarning(
                        "Circuit breaker opened due to {ExceptionType}: {ExceptionMessage}. " +
                        "Will remain open for {DurationSeconds} seconds",
                        exception?.GetType().Name ?? "HttpError",
                        exception?.Message ?? result.Result?.ReasonPhrase,
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
    }
}
