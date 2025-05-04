namespace WiredBrain.Ordering.Services;

using WiredBrain.Ordering.Models;

public class QueueConfigurationService
{
    private QueueConfiguration _configuration = new();

    public QueueConfiguration GetConfiguration() => _configuration;

    public void UpdateConfiguration(QueueConfiguration configuration)
    {
        _configuration.OrderArrivalDelayMs = configuration.OrderArrivalDelayMs;
        _configuration.BillingProcessingDelayMs = configuration.BillingProcessingDelayMs;
        _configuration.BillingServiceCount = configuration.BillingServiceCount;
    }
}
