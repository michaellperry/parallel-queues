using Microsoft.AspNetCore.Mvc;
using WiredBrain.Billing.Services;

namespace WiredBrain.Billing.Controllers;

[ApiController]
[Route("[controller]")]
public class BillingController : ControllerBase
{
    private readonly ILogger<BillingController> _logger;
    private readonly BillingRepository _billingRepository;

    public BillingController(ILogger<BillingController> logger, BillingRepository billingRepository)
    {
        _logger = logger;
        _billingRepository = billingRepository;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Billing Service is running.");
    }

    [HttpGet("total")]
    public IActionResult GetTotal()
    {
        var (totalAmount, transactionCount, lastUpdated) = _billingRepository.GetTotals();
        
        return Ok(new
        {
            TotalAmount = totalAmount,
            TransactionCount = transactionCount,
            LastUpdated = lastUpdated
        });
    }
}
