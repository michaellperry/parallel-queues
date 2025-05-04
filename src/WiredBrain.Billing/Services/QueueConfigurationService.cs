namespace WiredBrain.Billing.Services;

/// <summary>
/// A simplified version of the QueueConfigurationService for the Billing service
/// </summary>
public class QueueConfigurationService
{
    private int _billingServiceCount = 16;
    private double _coefficientOfServiceVariation = 0;

    /// <summary>
    /// Gets or sets the number of billing services available to process orders.
    /// Default: 16
    /// </summary>
    public int BillingServiceCount
    {
        get => _billingServiceCount;
        set => _billingServiceCount = value > 0 ? value : 16;
    }

    /// <summary>
    /// Gets or sets the coefficient of service variation (cs).
    /// This represents the variability in service rates.
    /// Default: 0 (no variance)
    /// </summary>
    public double CoefficientOfServiceVariation
    {
        get => _coefficientOfServiceVariation;
        set => _coefficientOfServiceVariation = value >= 0 ? value : 0;
    }

    /// <summary>
    /// Calculates a random processing time for billing based on the coefficient of service variation (cs)
    /// </summary>
    /// <param name="baseProcessingDelayMs">The base processing delay in milliseconds</param>
    /// <returns>The adjusted processing time in milliseconds</returns>
    public int GetRandomizedBillingProcessingDelay(int baseProcessingDelayMs)
    {
        if (CoefficientOfServiceVariation <= 0)
        {
            return baseProcessingDelayMs;
        }

        // Use Kingman's formula to adjust the processing time
        var random = new Random();
        
        // Generate a random factor based on the coefficient of variation
        // Using a normal distribution centered around 1.0
        var standardDeviation = CoefficientOfServiceVariation;
        
        // Box-Muller transform for normal distribution
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        
        // Apply the variation factor (centered around 1.0)
        var variationFactor = 1.0 + (z * standardDeviation);
        
        // Ensure we don't get negative processing times
        variationFactor = Math.Max(0.1, variationFactor);
        
        return (int)(baseProcessingDelayMs * variationFactor);
    }
}