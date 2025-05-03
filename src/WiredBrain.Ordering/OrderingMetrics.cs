using Prometheus;

namespace WiredBrain.Ordering;

public static class OrderingMetrics
{
    // Create a counter metric for tracking orders placed
    private static readonly Counter OrdersPlacedCounter = Metrics
        .CreateCounter("orders_placed_total", "Counts the total number of orders placed");

    // Increment the counter when an order is placed
    public static void TrackOrderPlaced()
    {
        OrdersPlacedCounter.Inc();
    }
}