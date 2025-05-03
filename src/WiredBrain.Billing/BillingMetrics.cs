using Prometheus;

namespace WiredBrain.Billing;

public static class BillingMetrics
{
    // Create a histogram metric for tracking total time in system (W)
    private static readonly Histogram WaitTimeHistogram = Metrics
        .CreateHistogram("billing_wait_time_seconds", "Tracks the total time in system (W) in seconds", new HistogramConfiguration
        {
            // Define buckets for the histogram
            Buckets = Histogram.LinearBuckets(0, 0.5, 20) // 20 buckets starting at 0, with width 0.5 seconds
        });

    // Track total time in system (W) (from order creation to processing completion)
    public static void TrackWaitTime(double waitTime)
    {
        WaitTimeHistogram.Observe(waitTime); // Record the wait time to the histogram
    }

    // Create a histogram metric for tracking processing time (τ)
    private static readonly Histogram ProcessingTimeHistogram = Metrics
        .CreateHistogram("billing_processing_time_seconds", "Tracks the processing time (τ) in seconds", new HistogramConfiguration
        {
            // Define buckets for the histogram
            Buckets = Histogram.LinearBuckets(0, 0.1, 20) // 20 buckets starting at 0, with width 0.1 seconds
        });

    // Create a counter metric for tracking processed orders (throughput)
    private static readonly Counter ProcessedCounter = Metrics
        .CreateCounter("billing_processed_total", "Counts the total number of processed orders");

    // Track processing time (τ) and increment processed counter
    public static void TrackProcessingTime(double timeInSeconds)
    {
        ProcessingTimeHistogram.Observe(timeInSeconds); // Record the processing time to the histogram
        ProcessedCounter.Inc(); // Increment the counter each time an order is processed
    }
}