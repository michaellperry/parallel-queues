using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using Polly.Timeout;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Services;

public class PaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentServiceClient> _logger;
    private readonly ResilienceConfig _resilienceConfig;

    public PaymentServiceClient(
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentServiceClient> logger,
        IOptions<ResilienceConfig> resilienceOptions)
    {
        _httpClient = httpClientFactory.CreateClient("PaymentService");
        _logger = logger;
        _resilienceConfig = resilienceOptions.Value;
        
        _logger.LogInformation(
            "PaymentServiceClient configured with timeout: {TimeoutSeconds}s, " +
            "max retry attempts: {MaxRetryAttempts}, " +
            "initial backoff: {InitialBackoffSeconds}s, " +
            "circuit breaker exceptions allowed: {ExceptionsAllowedBeforeBreaking}, " +
            "circuit breaker break duration: {DurationOfBreakSeconds}s",
            _resilienceConfig.TimeoutSeconds,
            _resilienceConfig.MaxRetryAttempts,
            _resilienceConfig.InitialBackoffSeconds,
            _resilienceConfig.ExceptionsAllowedBeforeBreaking,
            _resilienceConfig.DurationOfBreakSeconds);
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId} with amount {Amount:C}",
                request.OrderId, request.Amount);
            
            var response = await _httpClient.PostAsJsonAsync("api/payment/process", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<PaymentResponse>()
                ?? throw new InvalidOperationException("Failed to deserialize payment response");
            
            // Track successful request
            ResilienceMetrics.TrackRequestSuccess();
            
            _logger.LogInformation("Successfully processed payment for order {OrderId}. Payment ID: {PaymentId}",
                request.OrderId, result.PaymentId);
            return result;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex,
                "Circuit breaker is open - payment service is unavailable. Order {OrderId} payment could not be processed. " +
                "Current circuit state: {CircuitState}",
                request.OrderId, Policies.CircuitBreakerPolicy.CircuitState);
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"Payment service is unavailable (Circuit: {Policies.CircuitBreakerPolicy.CircuitState}). " +
                $"Order {request.OrderId} payment could not be processed.",
                ex, request.OrderId.ToString());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error processing payment for order {OrderId}. Status code: {StatusCode}, Host: {Host}, Endpoint: {Endpoint}",
                request.OrderId, ex.StatusCode, _httpClient.BaseAddress, "api/payment/process");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"HTTP error {ex.StatusCode} when processing payment for order {request.OrderId}.",
                ex, request.OrderId.ToString());
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex,
                "Timeout processing payment for order {OrderId} after {TimeoutSeconds}s. Host: {Host}, Endpoint: {Endpoint}",
                request.OrderId, _resilienceConfig.TimeoutSeconds, _httpClient.BaseAddress, "api/payment/process");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"Timeout after {_resilienceConfig.TimeoutSeconds}s when processing payment for order {request.OrderId}.",
                ex, request.OrderId.ToString());
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "Request canceled while processing payment for order {OrderId}. Host: {Host}, Endpoint: {Endpoint}",
                request.OrderId, _httpClient.BaseAddress, "api/payment/process");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"Request canceled when processing payment for order {request.OrderId}.",
                ex, request.OrderId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error processing payment for order {OrderId}. Host: {Host}, Endpoint: {Endpoint}",
                request.OrderId, _httpClient.BaseAddress, "api/payment/process");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"Unexpected error when processing payment for order {request.OrderId}: {ex.Message}",
                ex, request.OrderId.ToString());
        }
    }

    public async Task<TotalAmountResponse> GetTotalAmountAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving total amount from payment service");
            
            var response = await _httpClient.GetAsync("api/payment/total");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TotalAmountResponse>()
                ?? throw new InvalidOperationException("Failed to deserialize total amount response");
            
            // Track successful request
            ResilienceMetrics.TrackRequestSuccess();
            
            _logger.LogInformation("Successfully retrieved total amount: {TotalAmount:C}", result.TotalAmount);
            return result;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex,
                "Circuit breaker is open - payment service is unavailable. Could not retrieve total amount. " +
                "Current circuit state: {CircuitState}",
                Policies.CircuitBreakerPolicy.CircuitState);
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"Payment service is unavailable (Circuit: {Policies.CircuitBreakerPolicy.CircuitState}). " +
                "Could not retrieve total amount.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error retrieving total amount. Status code: {StatusCode}, Host: {Host}, Endpoint: {Endpoint}",
                ex.StatusCode, _httpClient.BaseAddress, "api/payment/total");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"HTTP error {ex.StatusCode} when retrieving total amount.", ex);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex,
                "Timeout retrieving total amount after {TimeoutSeconds}s. Host: {Host}, Endpoint: {Endpoint}",
                _resilienceConfig.TimeoutSeconds, _httpClient.BaseAddress, "api/payment/total");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"Timeout after {_resilienceConfig.TimeoutSeconds}s when retrieving total amount.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "Request canceled while retrieving total amount. Host: {Host}, Endpoint: {Endpoint}",
                _httpClient.BaseAddress, "api/payment/total");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                "Request canceled when retrieving total amount.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error retrieving total amount from payment service. Host: {Host}, Endpoint: {Endpoint}",
                _httpClient.BaseAddress, "api/payment/total");
            
            // Wrap the exception with more context
            throw new PaymentServiceException(
                $"Unexpected error when retrieving total amount: {ex.Message}", ex);
        }
    }
}

// Custom exception with additional context
public class PaymentServiceException : Exception
{
    public string? OrderId { get; }
    
    public PaymentServiceException(string message, Exception innerException, string? orderId = null)
        : base(message, innerException)
    {
        OrderId = orderId;
    }
}
