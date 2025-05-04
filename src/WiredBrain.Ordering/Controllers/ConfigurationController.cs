using Microsoft.AspNetCore.Mvc;
using WiredBrain.Ordering.Models;
using WiredBrain.Ordering.Services;

namespace WiredBrain.Ordering.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly QueueConfigurationService _configService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(QueueConfigurationService configService, ILogger<ConfigurationController> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current queue configuration
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(QueueConfiguration), StatusCodes.Status200OK)]
    public IActionResult GetConfiguration()
    {
        return Ok(_configService.GetConfiguration());
    }

    /// <summary>
    /// Updates the queue configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(QueueConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateConfiguration([FromBody] QueueConfiguration configuration)
    {
        if (configuration == null)
        {
            return BadRequest("Configuration cannot be null");
        }

        if (configuration.OrderArrivalDelayMs <= 0)
        {
            return BadRequest("OrderArrivalDelayMs must be greater than 0");
        }

        if (configuration.BillingProcessingDelayMs <= 0)
        {
            return BadRequest("BillingProcessingDelayMs must be greater than 0");
        }

        if (configuration.BillingServiceCount <= 0)
        {
            return BadRequest("BillingServiceCount must be greater than 0");
        }

        _configService.UpdateConfiguration(configuration);
        
        _logger.LogInformation(
            "Configuration updated: OrderArrivalDelayMs={OrderArrivalRateMs}, BillingProcessingDelayMs={BillingProcessingDelayMs}, BillingServiceCount={BillingServiceCount}",
            configuration.OrderArrivalDelayMs,
            configuration.BillingProcessingDelayMs,
            configuration.BillingServiceCount);

        return Ok(_configService.GetConfiguration());
    }

    /// <summary>
    /// Sets a predefined scenario configuration
    /// </summary>
    /// <param name="scenarioName">The name of the scenario to apply</param>
    /// <param name="billingServiceCount">Optional: The number of billing services available (default: 4)</param>
    [HttpPost("scenario/{scenarioName}")]
    [ProducesResponseType(typeof(QueueConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SetScenario(string scenarioName, [FromQuery] int? billingServiceCount = null)
    {
        var config = new QueueConfiguration();
        
        // If billingServiceCount is provided, use it; otherwise, use the default (4)
        if (billingServiceCount.HasValue && billingServiceCount.Value > 0)
        {
            config.BillingServiceCount = billingServiceCount.Value;
        }
        
        switch (scenarioName.ToLowerInvariant())
        {
            case "underload":
                // Scenario A: Underload (λ < cμ)
                config.OrderArrivalDelayMs = 2000; // λ = 0.5 orders/sec
                config.BillingProcessingDelayMs = 200; // μ = 5 orders/sec per service
                break;
                
            case "nearcapacity":
                // Scenario B: Near Capacity (λ ≈ cμ)
                config.OrderArrivalDelayMs = 350; // λ = 2.86 orders/sec
                config.BillingProcessingDelayMs = 200; // μ = 5 orders/sec per service
                break;
                
            case "overload":
                // Scenario C: Overload (λ > cμ)
                config.OrderArrivalDelayMs = 250; // λ = 4 orders/sec
                config.BillingProcessingDelayMs = 200; // μ = 5 orders/sec per service
                break;
                
            case "servicetime":
                // Scenario D: Impact of Service Time
                config.OrderArrivalDelayMs = 300; // λ = 3.33 orders/sec
                config.BillingProcessingDelayMs = 200; // μ = 5 orders/sec per service
                break;
                
            case "bottleneck":
                // Scenario E: Bottleneck Identification
                config.OrderArrivalDelayMs = 300; // λ = 3.33 orders/sec
                config.BillingProcessingDelayMs = 400; // μ = 2.5 orders/sec per service
                break;
                
            default:
                return BadRequest($"Unknown scenario: {scenarioName}. Available scenarios: underload, nearcapacity, overload, servicetime, bottleneck");
        }
        
        _configService.UpdateConfiguration(config);
        
        _logger.LogInformation(
            "Scenario {ScenarioName} applied: OrderArrivalRateMs={OrderArrivalRateMs}, BillingProcessingDelayMs={BillingProcessingDelayMs}, BillingServiceCount={BillingServiceCount}",
            scenarioName,
            config.OrderArrivalDelayMs,
            config.BillingProcessingDelayMs,
            config.BillingServiceCount);
            
        return Ok(_configService.GetConfiguration());
    }
}
