using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class CircuitBreakerPolicy
{
    // Expose the circuit breaker for health checks
    public static CircuitState CircuitState { get; private set; } = CircuitState.Closed;

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
                    CircuitState = CircuitState.Open;
                    ResilienceMetrics.TrackCircuitBreakerState(CircuitState);
                    
                    logger.LogWarning(
                        "Circuit breaker opened due to {ExceptionType}: {ExceptionMessage}. " +
                        "Will remain open for {DurationSeconds} seconds",
                        exception?.GetType().Name ?? "HttpError",
                        exception?.Message,
                        duration.TotalSeconds);
                    
                    // Track the failure that caused the circuit to break
                    if (exception is HttpRequestException httpEx)
                    {
                        ResilienceMetrics.TrackRequestFailure(httpEx, httpEx.StatusCode);
                    }
                    else
                    {
                        ResilienceMetrics.TrackRequestFailure(exception);
                    }
                },
                onReset: () =>
                {
                    CircuitState = CircuitState.Closed;
                    ResilienceMetrics.TrackCircuitBreakerState(CircuitState);
                    
                    logger.LogInformation("Circuit breaker closed. Payment service calls will resume");
                },
                onHalfOpen: () =>
                {
                    CircuitState = CircuitState.HalfOpen;
                    ResilienceMetrics.TrackCircuitBreakerState(CircuitState);
                    
                    logger.LogInformation("Circuit breaker half-open. Testing payment service availability");
                });
                
        return policy.AsAsyncPolicy<HttpResponseMessage>();
    }
}
