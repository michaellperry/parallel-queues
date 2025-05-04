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

        if (configuration.OrderArrivalRateMs <= 0)
        {
            return BadRequest("OrderArrivalRateMs must be greater than 0");
        }

        if (configuration.BillingProcessingDelayMs <= 0)
        {
            return BadRequest("BillingProcessingDelayMs must be greater than 0");
        }

        _configService.UpdateConfiguration(configuration);
        
        _logger.LogInformation(
            "Configuration updated: OrderArrivalRateMs={OrderArrivalRateMs}, BillingProcessingDelayMs={BillingProcessingDelayMs}",
            configuration.OrderArrivalRateMs,
            configuration.BillingProcessingDelayMs);

        return Ok(_configService.GetConfiguration());
    }

    /// <summary>
    /// Sets a predefined scenario configuration
    /// </summary>
    [HttpPost("scenario/{scenarioName}")]
    [ProducesResponseType(typeof(QueueConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SetScenario(string scenarioName)
    {
        var config = new QueueConfiguration();
        
        switch (scenarioName.ToLowerInvariant())
        {
            case "underload":
                // Scenario A: Underload (λ < 1/τ)
                config.OrderArrivalRateMs = 2000; // λ = 0.5 orders/sec
                config.BillingProcessingDelayMs = 200; // service rate = 5 orders/sec
                break;
                
            case "nearcapacity":
                // Scenario B: Near Capacity (λ ≈ 1/τ)
                config.OrderArrivalRateMs = 350; // λ = 2.86 orders/sec
                config.BillingProcessingDelayMs = 200; // service rate = 5 orders/sec
                break;
                
            case "overload":
                // Scenario C: Overload (λ > 1/τ)
                config.OrderArrivalRateMs = 250; // λ = 4 orders/sec
                config.BillingProcessingDelayMs = 200; // service rate = 5 orders/sec
                break;
                
            case "servicetime":
                // Scenario D: Impact of Service Time
                config.OrderArrivalRateMs = 300; // λ = 3.33 orders/sec
                config.BillingProcessingDelayMs = 200; // service rate = 5 orders/sec
                break;
                
            case "bottleneck":
                // Scenario E: Bottleneck Identification
                config.OrderArrivalRateMs = 300; // λ = 3.33 orders/sec
                config.BillingProcessingDelayMs = 400; // service rate = 2.5 orders/sec
                break;
                
            default:
                return BadRequest($"Unknown scenario: {scenarioName}. Available scenarios: underload, nearcapacity, overload, servicetime, bottleneck");
        }
        
        _configService.UpdateConfiguration(config);
        
        _logger.LogInformation(
            "Scenario {ScenarioName} applied: OrderArrivalRateMs={OrderArrivalRateMs}, BillingProcessingDelayMs={BillingProcessingDelayMs}",
            scenarioName,
            config.OrderArrivalRateMs,
            config.BillingProcessingDelayMs);
            
        return Ok(_configService.GetConfiguration());
    }
}