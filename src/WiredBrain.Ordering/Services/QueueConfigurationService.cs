namespace WiredBrain.Ordering.Services;

using WiredBrain.Ordering.Models;

public class QueueConfigurationService
{
    private QueueConfiguration _configuration;

    public QueueConfigurationService()
    {
        // Initialize with default values or from environment variables if available
        _configuration = new QueueConfiguration();
        
        var orderArrivalRateMs = Environment.GetEnvironmentVariable("ORDER_ARRIVAL_RATE_MS");
        if (!string.IsNullOrEmpty(orderArrivalRateMs) && int.TryParse(orderArrivalRateMs, out var orderRate))
        {
            _configuration.OrderArrivalRateMs = orderRate;
        }
        
        var billingProcessingDelayMs = Environment.GetEnvironmentVariable("BILLING_PROCESSING_DELAY_MS");
        if (!string.IsNullOrEmpty(billingProcessingDelayMs) && int.TryParse(billingProcessingDelayMs, out var billingDelay))
        {
            _configuration.BillingProcessingDelayMs = billingDelay;
        }
    }

    public QueueConfiguration GetConfiguration() => _configuration;

    public void UpdateConfiguration(QueueConfiguration configuration)
    {
        _configuration.OrderArrivalRateMs = configuration.OrderArrivalRateMs;
        _configuration.BillingProcessingDelayMs = configuration.BillingProcessingDelayMs;
    }
}