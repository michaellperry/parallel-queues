using System.Collections.Concurrent;
using WiredBrain.Payment.Models;

namespace WiredBrain.Payment.Services;

public class PaymentRepository
{
    private readonly ConcurrentDictionary<Guid, PaymentRecord> _payments = new();
    private decimal _totalAmount = 0;
    private readonly object _totalAmountLock = new();
    private DateTime _lastUpdated = DateTime.UtcNow;

    public Task<PaymentRecord?> GetPaymentByOrderId(Guid orderId)
    {
        var payment = _payments.Values.FirstOrDefault(p => p.OrderId == orderId);
        return Task.FromResult(payment);
    }

    public Task<PaymentRecord> StorePayment(PaymentRecord payment)
    {
        _payments.TryAdd(payment.PaymentId, payment);
        
        // Thread-safe update of the total amount
        lock (_totalAmountLock)
        {
            _totalAmount += payment.Amount;
            _lastUpdated = DateTime.UtcNow;
        }
        
        return Task.FromResult(payment);
    }

    public Task<decimal> GetTotalAmount()
    {
        lock (_totalAmountLock)
        {
            return Task.FromResult(_totalAmount);
        }
    }

    public Task<int> GetTransactionCount()
    {
        return Task.FromResult(_payments.Count);
    }
    
    public Task<DateTime> GetLastUpdated()
    {
        lock (_totalAmountLock)
        {
            return Task.FromResult(_lastUpdated);
        }
    }
}