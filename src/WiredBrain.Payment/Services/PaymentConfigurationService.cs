using WiredBrain.Payment.Models;

namespace WiredBrain.Payment.Services;

public class PaymentConfigurationService
{
    private PaymentConfiguration _configuration;
    private readonly object _lock = new object();
    
    public PaymentConfigurationService(IConfiguration configuration)
    {
        // Initialize from appsettings.json
        _configuration = new PaymentConfiguration
        {
            ProcessingDelayMs = configuration.GetValue<int>("PaymentService:DefaultProcessingDelayMs", 200)
        };
    }
    
    public PaymentConfiguration GetConfiguration()
    {
        lock (_lock)
        {
            return _configuration;
        }
    }
    
    public PaymentConfiguration UpdateConfiguration(PaymentConfiguration newConfig)
    {
        lock (_lock)
        {
            _configuration = newConfig;
            _configuration.UpdatedAt = DateTime.UtcNow;
            return _configuration;
        }
    }
}