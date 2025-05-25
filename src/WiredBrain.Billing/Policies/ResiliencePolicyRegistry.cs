using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Registry;
using Polly.Timeout;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public class ResiliencePolicyRegistry
{
    private readonly ILogger<ResiliencePolicyRegistry> _logger;
    private readonly ResilienceConfig _config;
    private readonly PolicyRegistry _registry = new();

    public ResiliencePolicyRegistry(ILogger<ResiliencePolicyRegistry> logger, IOptions<ResilienceConfig> options)
    {
        _logger = logger;
        _config = options.Value;
        
        // Register individual policies
        RegisterTimeoutPolicy();
        RegisterRetryPolicy();
        RegisterCircuitBreakerPolicy();
        
        // Register policy wrap with correct ordering
        RegisterPolicyWrap();
    }

    public IReadOnlyPolicyRegistry<string> Registry => _registry;

    private void RegisterTimeoutPolicy()
    {
        _registry.Add("PaymentService.Timeout", Policy.TimeoutAsync<HttpResponseMessage>(_config.Timeout));
    }

    private void RegisterRetryPolicy()
    {
        // Use the DecorrelatedJitterBackoffV2 formula for smoother distribution of retry intervals
        var delay = Polly.Contrib.WaitAndRetry.Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: _config.InitialBackoff,
            retryCount: _config.MaxRetryAttempts);

        _registry.Add("PaymentService.Retry", HttpPolicyExtensions
            .HandleTransientHttpError() // HttpRequestException, 5XX and 408 status codes
            .Or<TimeoutRejectedException>() // Handle timeout rejections
            .WaitAndRetryAsync(
                delay,
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryAttempt} after {TimespanSeconds}s delay due to {Message}",
                        retryAttempt,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                }
            ));
    }

    private void RegisterCircuitBreakerPolicy()
    {
        _logger.LogWarning("Configuring circuit breaker policy with " +
                          "{ExceptionsAllowedBeforeBreaking} exceptions allowed before breaking, " +
                          "{DurationOfBreak} seconds duration of break",
            _config.ExceptionsAllowedBeforeBreaking,
            _config.DurationOfBreak.TotalSeconds);
            
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: _config.ExceptionsAllowedBeforeBreaking,
                durationOfBreak: _config.DurationOfBreak,
                onBreak: (exception, duration) =>
                {
                    _logger.LogWarning(
                        "Circuit breaker opened due to {ExceptionType}: {ExceptionMessage}. " +
                        "Will remain open for {DurationSeconds} seconds",
                        exception?.GetType().Name,
                        exception?.Message,
                        duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker closed. Payment service calls will resume");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit breaker half-open. Testing payment service availability");
                });
                
        _registry.Add("PaymentService.CircuitBreaker", circuitBreakerPolicy);
    }

    private void RegisterPolicyWrap()
    {
        // Create a combined policy wrap with the correct ordering:
        // Circuit Breaker (outermost) -> Retry -> Timeout (innermost)
        _registry.Add("PaymentService.PolicyWrap", Policy.WrapAsync(
            _registry.Get<IAsyncPolicy<HttpResponseMessage>>("PaymentService.CircuitBreaker"),
            _registry.Get<IAsyncPolicy<HttpResponseMessage>>("PaymentService.Retry"),
            _registry.Get<IAsyncPolicy<HttpResponseMessage>>("PaymentService.Timeout")
        ));
    }
}