using Prometheus;

namespace WiredBrain.Billing;

public static class BillingMetrics
{
    // Create a histogram metric for tracking wait time
    private static readonly Histogram WaitTimeHistogram = Metrics
        .CreateHistogram("wait_time", "Tracks the wait time in seconds", new HistogramConfiguration
        {
            // Optional: Define buckets for the histogram. Customize as needed.
            Buckets = Histogram.LinearBuckets(0, 0.1, 10) // Example: 10 buckets starting at 0, with width 0.1
        });

    // Increment and track wait time (e.g., when an order is placed)
    public static void TrackWaitTime(double waitTime)
    {
        WaitTimeHistogram.Observe(waitTime); // Record the wait time to the histogram
    }

    // Create a counter metric for tracking processing time
    private static readonly Counter ProcessingTimeCounter = Metrics
        .CreateCounter("processing_time", "Counts the processing time in milliseconds");

    // Increment and track processing time (e.g., after a task has been processed)
    public static void TrackProcessingTime(double time)
    {
        ProcessingTimeCounter.Inc(); // Increment the counter (each time an item is processed)
        WaitTimeHistogram.Observe(time); // Record the wait time to the histogram
    }
}