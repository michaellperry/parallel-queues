using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly.CircuitBreaker;

namespace WiredBrain.Billing.HealthChecks;

public class CircuitBreakerHealthCheck : IHealthCheck
{
    private readonly ILogger<CircuitBreakerHealthCheck> _logger;

    public CircuitBreakerHealthCheck(ILogger<CircuitBreakerHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var circuitState = Policies.CircuitBreakerPolicy.CircuitState;
        
        _logger.LogDebug("Circuit breaker health check - current state: {CircuitState}", circuitState);
        
        return Task.FromResult(circuitState switch
        {
            CircuitState.Closed => HealthCheckResult.Healthy(
                "Payment service circuit breaker is closed. Service is operating normally."),
                
            CircuitState.HalfOpen => HealthCheckResult.Degraded(
                "Payment service circuit breaker is half-open. Service is recovering from failure."),
                
            CircuitState.Open => HealthCheckResult.Unhealthy(
                "Payment service circuit breaker is open. Service is unavailable."),
                
            _ => HealthCheckResult.Unhealthy(
                $"Payment service circuit breaker is in an unknown state: {circuitState}")
        });
    }
}