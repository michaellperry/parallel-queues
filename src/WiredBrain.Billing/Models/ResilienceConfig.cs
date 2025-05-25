using System;

namespace WiredBrain.Billing.Models;

public class ResilienceConfig
{
    public int TimeoutSeconds { get; set; } = 5;
    public int MaxRetryAttempts { get; set; } = 3;
    public int InitialBackoffSeconds { get; set; } = 1;
    public double BackoffMultiplier { get; set; } = 2.0;
    public double JitterFactor { get; set; } = 0.2;

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutSeconds);
    public TimeSpan InitialBackoff => TimeSpan.FromSeconds(InitialBackoffSeconds);
}