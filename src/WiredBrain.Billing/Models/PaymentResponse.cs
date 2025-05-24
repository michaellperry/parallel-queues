namespace WiredBrain.Billing.Models;

public class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public long ProcessingTimeMs { get; set; }
}