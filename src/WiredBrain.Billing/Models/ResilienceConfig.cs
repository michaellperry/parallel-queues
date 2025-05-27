namespace WiredBrain.Billing.Models;

public class ResilienceConfig
{
    public int TimeoutSeconds { get; set; } = 5;
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// The median first retry delay in seconds.
    /// Used by DecorrelatedJitterBackoffV2 to calculate retry delays.
    /// </summary>
    public int InitialBackoffSeconds { get; set; } = 1;

    /// <summary>
    /// Number of consecutive exceptions allowed before the circuit breaker opens.
    /// </summary>
    public int ExceptionsAllowedBeforeBreaking { get; set; } = 3;
    
    /// <summary>
    /// Duration in seconds that the circuit breaker stays open before transitioning to half-open.
    /// </summary>
    public int DurationOfBreakSeconds { get; set; } = 30;

    // Kill Switch Configuration
    /// <summary>
    /// Minimum number of messages to track before the kill switch can be activated.
    /// </summary>
    public int KillSwitchActivationThreshold { get; set; } = 10;
    
    /// <summary>
    /// Failure rate threshold (0.0-1.0) that triggers the kill switch.
    /// </summary>
    public double KillSwitchTripThreshold { get; set; } = 0.15;
    
    /// <summary>
    /// Duration in seconds before the kill switch automatically restarts.
    /// </summary>
    public int KillSwitchRestartTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Duration in minutes for the tracking period of the kill switch.
    /// </summary>
    public int KillSwitchTrackingPeriodMinutes { get; set; } = 1;

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutSeconds);
    public TimeSpan InitialBackoff => TimeSpan.FromSeconds(InitialBackoffSeconds);
    public TimeSpan DurationOfBreak => TimeSpan.FromSeconds(DurationOfBreakSeconds);
    public TimeSpan KillSwitchRestartTimeout => TimeSpan.FromSeconds(KillSwitchRestartTimeoutSeconds);
    public TimeSpan KillSwitchTrackingPeriod => TimeSpan.FromMinutes(KillSwitchTrackingPeriodMinutes);
}
