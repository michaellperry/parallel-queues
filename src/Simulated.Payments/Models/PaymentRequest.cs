namespace Simulated.Payments.Models;

public class PaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}