using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Services;

public class PaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentServiceClient> _logger;

    public PaymentServiceClient(IHttpClientFactory httpClientFactory, ILogger<PaymentServiceClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("PaymentService");
        _logger = logger;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/payment/process", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PaymentResponse>() 
                ?? throw new InvalidOperationException("Failed to deserialize payment response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
            throw;
        }
    }

    public async Task<TotalAmountResponse> GetTotalAmountAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/payment/total");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TotalAmountResponse>() 
                ?? throw new InvalidOperationException("Failed to deserialize total amount response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total amount from payment service");
            throw;
        }
    }
}