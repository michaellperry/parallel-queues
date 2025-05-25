using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
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
        _registry.Add("PaymentService.Timeout",
            TimeoutPolicy.Factory(_logger, _config));
    }

    private void RegisterRetryPolicy()
    {
        _registry.Add("PaymentService.Retry",
            RetryPolicy.Factory(_logger, _config));
    }

    private void RegisterCircuitBreakerPolicy()
    {
        _registry.Add("PaymentService.CircuitBreaker",
            CircuitBreakerPolicy.Factory(_logger, _config));
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
        
        _logger.LogInformation("Resilience policy wrap registered with Circuit Breaker -> Retry -> Timeout ordering");
    }
}