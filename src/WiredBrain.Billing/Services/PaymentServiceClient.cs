using System.Net;
using Microsoft.Extensions.Options;
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
            "backoff multiplier: {BackoffMultiplier}, " +
            "jitter factor: {JitterFactor}",
            _resilienceConfig.TimeoutSeconds,
            _resilienceConfig.MaxRetryAttempts,
            _resilienceConfig.InitialBackoffSeconds,
            _resilienceConfig.BackoffMultiplier,
            _resilienceConfig.JitterFactor);
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId}", request.OrderId);
            
            var response = await _httpClient.PostAsJsonAsync("api/payment/process", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<PaymentResponse>()
                ?? throw new InvalidOperationException("Failed to deserialize payment response");
            
            _logger.LogInformation("Successfully processed payment for order {OrderId}", request.OrderId);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error processing payment for order {OrderId}. Status code: {StatusCode}",
                request.OrderId, ex.StatusCode);
            throw;
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout processing payment for order {OrderId} after {TimeoutSeconds}s",
                request.OrderId, _resilienceConfig.TimeoutSeconds);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request canceled while processing payment for order {OrderId}", request.OrderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment for order {OrderId}", request.OrderId);
            throw;
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
            
            _logger.LogInformation("Successfully retrieved total amount: {TotalAmount}", result.TotalAmount);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error retrieving total amount. Status code: {StatusCode}", ex.StatusCode);
            throw;
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout retrieving total amount after {TimeoutSeconds}s",
                _resilienceConfig.TimeoutSeconds);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request canceled while retrieving total amount");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving total amount from payment service");
            throw;
        }
    }
}