namespace WiredBrain.Ordering.Models;

public class QueueConfiguration
{
    private int _orderArrivalRateMs = 1000;
    private int _billingProcessingDelayMs = 200;
    private int _billingServiceCount = 4;

    /// <summary>
    /// Gets or sets the delay between order placements in milliseconds.
    /// Default: 1000ms (1 order per second)
    /// </summary>
    public int OrderArrivalRateMs
    {
        get => _orderArrivalRateMs;
        set => _orderArrivalRateMs = value > 0 ? value : 1000;
    }

    /// <summary>
    /// Gets or sets the billing processing delay in milliseconds.
    /// Default: 200ms
    /// </summary>
    public int BillingProcessingDelayMs
    {
        get => _billingProcessingDelayMs;
        set => _billingProcessingDelayMs = value > 0 ? value : 200;
    }

    /// <summary>
    /// Gets or sets the number of billing services available to process orders.
    /// Default: 4
    /// </summary>
    public int BillingServiceCount
    {
        get => _billingServiceCount;
        set => _billingServiceCount = value > 0 ? value : 4;
    }
}
