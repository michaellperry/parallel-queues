using System.Diagnostics;
using MassTransit;
using WiredBrain.Billing.Models;
using WiredBrain.Billing.Services;
using WiredBrain.Messages;

namespace WiredBrain.Billing;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    private readonly PaymentServiceClient _paymentServiceClient;
    private readonly BillingRepository _billingRepository;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(
        PaymentServiceClient paymentServiceClient,
        BillingRepository billingRepository,
        ILogger<OrderPlacedConsumer> logger)
    {
        _paymentServiceClient = paymentServiceClient;
        _billingRepository = billingRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Processing payment for order: {order.OrderId} for {order.CustomerName} - ${order.Amount}");
        
        // Create payment request
        var paymentRequest = new PaymentRequest
        {
            OrderId = order.OrderId,
            Amount = order.Amount,
            CustomerName = order.CustomerName,
            OrderDate = order.OrderDate
        };

        try
        {
            // Measure payment processing time
            var paymentStopwatch = Stopwatch.StartNew();
            
            // Process payment through the payment service
            var paymentResponse = await _paymentServiceClient.ProcessPaymentAsync(paymentRequest);
            
            paymentStopwatch.Stop();
            
            // Record the charge in the repository
            _billingRepository.AddCharge(order.Amount);
            
            Console.WriteLine($"Payment processed successfully for order: {order.OrderId}, Payment ID: {paymentResponse.PaymentId} in {paymentStopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment for order {OrderId}", order.OrderId);
            throw; // Rethrow to trigger retry
        }
        
        stopwatch.Stop();
        
        // Convert milliseconds to seconds for the processing time metric
        double processingTimeSeconds = stopwatch.Elapsed.TotalSeconds;
        BillingMetrics.TrackProcessingTime(processingTimeSeconds);

        // Total wait time (W) includes both the time in queue and the processing time
        var waitTime = (DateTime.UtcNow - order.OrderDate).TotalSeconds;
        BillingMetrics.TrackWaitTime(waitTime);
    }
}
