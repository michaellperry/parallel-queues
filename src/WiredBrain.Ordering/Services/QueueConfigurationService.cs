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
        _configuration.CoefficientOfArrivalVariation = configuration.CoefficientOfArrivalVariation;
        _configuration.CoefficientOfServiceVariation = configuration.CoefficientOfServiceVariation;
    }

    /// <summary>
    /// Calculates a random delay for order arrival based on the coefficient of arrival variation (ca)
    /// </summary>
    /// <returns>The adjusted delay time in milliseconds</returns>
    public int GetRandomizedOrderArrivalDelay()
    {
        var config = GetConfiguration();
        
        if (config.CoefficientOfArrivalVariation <= 0)
        {
            return config.OrderArrivalDelayMs;
        }

        // Use Kingman's formula to adjust the delay time
        // For arrival times, we apply the variation to the base delay
        var random = new Random();
        var baseDelay = config.OrderArrivalDelayMs;
        
        // Generate a random factor based on the coefficient of variation
        // Using a normal distribution centered around 1.0
        var standardDeviation = config.CoefficientOfArrivalVariation;
        
        // Box-Muller transform for normal distribution
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        
        // Apply the variation factor (centered around 1.0)
        var variationFactor = 1.0 + (z * standardDeviation);
        
        // Ensure we don't get negative delays
        variationFactor = Math.Max(0.1, variationFactor);
        
        return (int)(baseDelay * variationFactor);
    }

    /// <summary>
    /// Calculates a random processing time for billing based on the coefficient of service variation (cs)
    /// </summary>
    /// <returns>The adjusted processing time in milliseconds</returns>
    public int GetRandomizedBillingProcessingDelay()
    {
        var config = GetConfiguration();
        
        if (config.CoefficientOfServiceVariation <= 0)
        {
            return config.BillingProcessingDelayMs;
        }

        // Use Kingman's formula to adjust the processing time
        var random = new Random();
        var baseDelay = config.BillingProcessingDelayMs;
        
        // Generate a random factor based on the coefficient of variation
        // Using a normal distribution centered around 1.0
        var standardDeviation = config.CoefficientOfServiceVariation;
        
        // Box-Muller transform for normal distribution
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        
        // Apply the variation factor (centered around 1.0)
        var variationFactor = 1.0 + (z * standardDeviation);
        
        // Ensure we don't get negative processing times
        variationFactor = Math.Max(0.1, variationFactor);
        
        return (int)(baseDelay * variationFactor);
    }

    /// <summary>
    /// Calculates the expected wait time using Kingman's formula
    /// </summary>
    /// <returns>The expected wait time in milliseconds</returns>
    public double CalculateExpectedWaitTime()
    {
        var config = GetConfiguration();
        
        // Calculate utilization (ρ)
        // ρ = λ / (c * μ) where:
        // λ = arrival rate (orders per ms) = 1 / OrderArrivalDelayMs
        // μ = service rate (orders per ms) = 1 / BillingProcessingDelayMs
        // c = number of servers (BillingServiceCount)
        
        double arrivalRate = 1.0 / config.OrderArrivalDelayMs;
        double serviceRate = 1.0 / config.BillingProcessingDelayMs;
        int servers = config.BillingServiceCount;
        
        double utilization = arrivalRate / (servers * serviceRate);
        
        // If utilization >= 1, the system is unstable (queue will grow infinitely)
        if (utilization >= 1)
        {
            return double.PositiveInfinity;
        }
        
        // Kingman's formula: Wait time ≈ (ca² + cs²) / 2 * (ρ / (1-ρ)) * service time
        double ca2 = Math.Pow(config.CoefficientOfArrivalVariation, 2);
        double cs2 = Math.Pow(config.CoefficientOfServiceVariation, 2);
        
        double waitTime = (ca2 + cs2) / 2 * (utilization / (1 - utilization)) * config.BillingProcessingDelayMs;
        
        return waitTime;
    }
}
