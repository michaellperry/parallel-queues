using Microsoft.AspNetCore.Mvc;
using WiredBrain.Payment.Models;
using WiredBrain.Payment.Services;

namespace WiredBrain.Payment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ILogger<PaymentController> _logger;
    private readonly PaymentProcessorService _paymentProcessor;
    private readonly PaymentRepository _repository;

    public PaymentController(
        ILogger<PaymentController> logger,
        PaymentProcessorService paymentProcessor,
        PaymentRepository repository)
    {
        _logger = logger;
        _paymentProcessor = paymentProcessor;
        _repository = repository;
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] PaymentRequest request)
    {
        if (request == null)
        {
            return BadRequest("Payment request cannot be null");
        }

        if (request.OrderId == Guid.Empty)
        {
            return BadRequest("Order ID is required");
        }

        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero");
        }

        try
        {
            var response = await _paymentProcessor.ProcessPayment(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
            return StatusCode(500, "An error occurred while processing the payment");
        }
    }

    [HttpGet("total")]
    public async Task<ActionResult<TotalAmountResponse>> GetTotalAmount()
    {
        try
        {
            var totalAmount = await _repository.GetTotalAmount();
            var transactionCount = await _repository.GetTransactionCount();
            var lastUpdated = await _repository.GetLastUpdated();

            var response = new TotalAmountResponse
            {
                TotalAmount = totalAmount,
                TransactionCount = transactionCount,
                LastUpdated = lastUpdated
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total amount");
            return StatusCode(500, "An error occurred while retrieving the total amount");
        }
    }
}