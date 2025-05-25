using System.Diagnostics;
using Simulated.Payments.Models;

namespace Simulated.Payments.Services;

public class PaymentProcessorService
{
    private readonly PaymentRepository _repository;
    private readonly ILogger<PaymentProcessorService> _logger;
    private readonly PaymentConfigurationService _configService;

    public PaymentProcessorService(
        PaymentRepository repository,
        ILogger<PaymentProcessorService> logger,
        PaymentConfigurationService configService)
    {
        _repository = repository;
        _logger = logger;
        _configService = configService;
    }

    public async Task<PaymentResponse> ProcessPayment(PaymentRequest request)
    {
        // Check for idempotency - if payment already exists, return it
        var existingPayment = await _repository.GetPaymentByOrderId(request.OrderId);
        if (existingPayment != null)
        {
            _logger.LogInformation("Payment for order {OrderId} already processed. Returning existing payment.", request.OrderId);
            
            return new PaymentResponse
            {
                PaymentId = existingPayment.PaymentId,
                OrderId = existingPayment.OrderId,
                Amount = existingPayment.Amount,
                Status = existingPayment.Status,
                ProcessedAt = existingPayment.ProcessedAt,
                ProcessingTimeMs = existingPayment.ProcessingTimeMs
            };
        }

        // Get the current configuration
        var config = _configService.GetConfiguration();
        
        // Process the payment
        _logger.LogInformation("Processing payment for order {OrderId} for {CustomerName} - ${Amount}", 
            request.OrderId, request.CustomerName, request.Amount);
        
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate payment processing with configurable delay and random jitter
        var random = new Random();
        var jitterMs = random.Next(-200, 201); // Random jitter between -200ms and +200ms
        var delayMs = Math.Max(1, config.ProcessingDelayMs + jitterMs); // Ensure delay is at least 1ms
        
        _logger.LogDebug("Payment processing delay: {BaseDelay}ms + {Jitter}ms jitter = {TotalDelay}ms", 
            config.ProcessingDelayMs, jitterMs, delayMs);
            
        await Task.Delay(delayMs);
        
        stopwatch.Stop();
        
        // Create payment record
        var paymentRecord = new PaymentRecord
        {
            PaymentId = Guid.NewGuid(),
            OrderId = request.OrderId,
            Amount = request.Amount,
            CustomerName = request.CustomerName,
            OrderDate = request.OrderDate,
            ProcessedAt = DateTime.UtcNow,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
            Status = "Succeeded"
        };
        
        // Store the payment
        await _repository.StorePayment(paymentRecord);
        
        _logger.LogInformation("Payment processed successfully for order {OrderId}. Payment ID: {PaymentId}", 
            request.OrderId, paymentRecord.PaymentId);
        
        // Return response
        return new PaymentResponse
        {
            PaymentId = paymentRecord.PaymentId,
            OrderId = paymentRecord.OrderId,
            Amount = paymentRecord.Amount,
            Status = paymentRecord.Status,
            ProcessedAt = paymentRecord.ProcessedAt,
            ProcessingTimeMs = paymentRecord.ProcessingTimeMs
        };
    }
}