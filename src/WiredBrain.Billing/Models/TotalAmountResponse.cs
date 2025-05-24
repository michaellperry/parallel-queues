namespace WiredBrain.Billing.Models;

public class TotalAmountResponse
{
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public DateTime LastUpdated { get; set; }
}