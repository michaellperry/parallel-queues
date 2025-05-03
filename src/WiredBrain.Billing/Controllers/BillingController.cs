using Microsoft.AspNetCore.Mvc;

namespace WiredBrain.Billing.Controllers;

[ApiController]
[Route("[controller]")]
public class BillingController : ControllerBase
{
    private readonly ILogger<BillingController> _logger;

    public BillingController(ILogger<BillingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Billing Service is running.");
    }
}
