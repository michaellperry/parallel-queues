namespace WiredBrain.Payment.Models;

public class PaymentRecord
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime ProcessedAt { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string Status { get; set; } = string.Empty;
}