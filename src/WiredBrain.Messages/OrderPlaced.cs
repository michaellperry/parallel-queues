namespace WiredBrain.Messages;

public record OrderPlaced
{
    public Guid OrderId { get; init; }
    public string CustomerName { get; init; } = "";
    public DateTime OrderDate { get; init; }
    public decimal Amount { get; init; }
    public int BillingProcessingDelayMs { get; init; } = 200;
    public double CoefficientOfServiceVariation { get; init; } = 0;
}
