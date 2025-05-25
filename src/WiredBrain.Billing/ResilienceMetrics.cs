using Prometheus;
using Polly.CircuitBreaker;

namespace WiredBrain.Billing;

public static class ResilienceMetrics
{
    // Circuit breaker state metrics
    private static readonly Gauge CircuitBreakerStateGauge = Metrics
        .CreateGauge("payment_circuit_breaker_state", 
            "Current state of the circuit breaker (0=Closed, 1=Open, 2=HalfOpen)");
    
    // Request metrics
    private static readonly Counter RequestSuccessCounter = Metrics
        .CreateCounter("payment_request_success_total", 
            "Total number of successful payment service requests");
    
    private static readonly Counter RequestFailureCounter = Metrics
        .CreateCounter("payment_request_failure_total", 
            "Total number of failed payment service requests", 
            new CounterConfiguration
            {
                LabelNames = new[] { "exception_type", "status_code" }
            });
    
    // Retry metrics
    private static readonly Counter RetryAttemptsCounter = Metrics
        .CreateCounter("payment_retry_attempts_total", 
            "Total number of retry attempts made to the payment service");
    
    private static readonly Histogram RetryDelayHistogram = Metrics
        .CreateHistogram("payment_retry_delay_seconds", 
            "Delay between retry attempts in seconds");
    
    // Timeout metrics
    private static readonly Counter TimeoutOccurrencesCounter = Metrics
        .CreateCounter("payment_timeout_total", 
            "Total number of timeout occurrences when calling the payment service");
    
    // Track circuit breaker state changes
    public static void TrackCircuitBreakerState(CircuitState state)
    {
        CircuitBreakerStateGauge.Set((int)state);
    }
    
    // Track successful requests
    public static void TrackRequestSuccess()
    {
        RequestSuccessCounter.Inc();
    }
    
    // Track failed requests
    public static void TrackRequestFailure(Exception? exception = null, System.Net.HttpStatusCode? statusCode = null)
    {
        string exceptionType = exception?.GetType().Name ?? "Unknown";
        string statusCodeValue = statusCode?.ToString() ?? "None";
        
        RequestFailureCounter.WithLabels(exceptionType, statusCodeValue).Inc();
    }
    
    // Track retry attempts
    public static void TrackRetryAttempt()
    {
        RetryAttemptsCounter.Inc();
    }
    
    // Track retry delay
    public static void TrackRetryDelay(TimeSpan delay)
    {
        RetryDelayHistogram.Observe(delay.TotalSeconds);
    }
    
    // Track timeout occurrences
    public static void TrackTimeout()
    {
        TimeoutOccurrencesCounter.Inc();
    }
}