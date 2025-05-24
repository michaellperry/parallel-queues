namespace WiredBrain.Payment.Models;

public class PaymentConfiguration
{
    public int ProcessingDelayMs { get; set; } = 200;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}