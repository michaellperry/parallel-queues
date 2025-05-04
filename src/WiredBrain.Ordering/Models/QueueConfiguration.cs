namespace WiredBrain.Ordering.Models;

public class QueueConfiguration
{
    private int _orderArrivalDelayMs = 300;
    private int _billingProcessingDelayMs = 500;
    private int _billingServiceCount = 16;

    /// <summary>
    /// Gets or sets the delay between order placements in milliseconds.
    /// Default: 300ms (3.3 order per second)
    /// </summary>
    public int OrderArrivalDelayMs
    {
        get => _orderArrivalDelayMs;
        set => _orderArrivalDelayMs = value > 0 ? value : 300;
    }

    /// <summary>
    /// Gets or sets the billing processing delay in milliseconds.
    /// Default: 500ms
    /// </summary>
    public int BillingProcessingDelayMs
    {
        get => _billingProcessingDelayMs;
        set => _billingProcessingDelayMs = value > 0 ? value : 500;
    }

    /// <summary>
    /// Gets or sets the number of billing services available to process orders.
    /// Default: 16
    /// </summary>
    public int BillingServiceCount
    {
        get => _billingServiceCount;
        set => _billingServiceCount = value > 0 ? value : 16;
    }
}
