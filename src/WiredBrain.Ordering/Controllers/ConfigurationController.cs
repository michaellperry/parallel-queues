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

        if (configuration.CoefficientOfArrivalVariation < 0)
        {
            return BadRequest("CoefficientOfArrivalVariation must be greater than or equal to 0");
        }

        if (configuration.CoefficientOfServiceVariation < 0)
        {
            return BadRequest("CoefficientOfServiceVariation must be greater than or equal to 0");
        }

        _configService.UpdateConfiguration(configuration);
        
        _logger.LogInformation(
            "Configuration updated: OrderArrivalDelayMs={OrderArrivalRateMs}, BillingProcessingDelayMs={BillingProcessingDelayMs}, BillingServiceCount={BillingServiceCount}, Ca={Ca}, Cs={Cs}",
            configuration.OrderArrivalDelayMs,
            configuration.BillingProcessingDelayMs,
            configuration.BillingServiceCount,
            configuration.CoefficientOfArrivalVariation,
            configuration.CoefficientOfServiceVariation);

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
                config.OrderArrivalDelayMs = 4800 / config.BillingServiceCount; // λ = 4.8 orders/sec per service
                config.BillingProcessingDelayMs = 500; // μ = 2 orders/sec for each service
                config.CoefficientOfArrivalVariation = 0;
                config.CoefficientOfServiceVariation = 0;
                break;
                
            case "nearcapacity":
                // Scenario B: Near Capacity (λ ≈ cμ)
                config.OrderArrivalDelayMs = 625 / config.BillingServiceCount; // λ = 1.6 orders/sec per service
                config.BillingProcessingDelayMs = 500; // μ = 2 orders/sec for each service
                config.CoefficientOfArrivalVariation = 0;
                config.CoefficientOfServiceVariation = 0;
                break;
                
            case "overload":
                // Scenario C: Overload (λ > cμ)
                config.OrderArrivalDelayMs = 450 / config.BillingServiceCount; // λ = 2.2 orders/sec per service
                config.BillingProcessingDelayMs = 500; // μ = 2 orders/sec for each service
                config.CoefficientOfArrivalVariation = 0;
                config.CoefficientOfServiceVariation = 0;
                break;
                
            case "lowvariability":
                // Scenario 1: Low Variability, Moderate Utilization (ca ≈ 0.5, cs ≈ 0.5, ρ ≈ 0.7)
                config.BillingProcessingDelayMs = 500; // μ = 2 orders/sec for each service
                // For ρ = 0.7, λ = 0.7 * c * μ = 0.7 * c * 2 = 1.4 * c orders/sec
                config.OrderArrivalDelayMs = (int)(1000 / (1.4 * config.BillingServiceCount));
                config.CoefficientOfArrivalVariation = 0.5;
                config.CoefficientOfServiceVariation = 0.5;
                break;
                
            case "highservicevariability":
                // Scenario 2: High Service Variability, Moderate Utilization (ca ≈ 0.5, cs ≈ 2.0, ρ ≈ 0.7)
                config.BillingProcessingDelayMs = 500; // μ = 2 orders/sec for each service
                // For ρ = 0.7, λ = 0.7 * c * μ = 0.7 * c * 2 = 1.4 * c orders/sec
                config.OrderArrivalDelayMs = (int)(1000 / (1.4 * config.BillingServiceCount));
                config.CoefficientOfArrivalVariation = 0.5;
                config.CoefficientOfServiceVariation = 2.0;
                break;
                
            case "highvariability":
                // Scenario 3: High Arrival and Service Variability, High Utilization (ca ≈ 1.5, cs ≈ 1.5, ρ ≈ 0.9)
                config.BillingProcessingDelayMs = 500; // μ = 2 orders/sec for each service
                // For ρ = 0.9, λ = 0.9 * c * μ = 0.9 * c * 2 = 1.8 * c orders/sec
                config.OrderArrivalDelayMs = (int)(1000 / (1.8 * config.BillingServiceCount));
                config.CoefficientOfArrivalVariation = 1.5;
                config.CoefficientOfServiceVariation = 1.5;
                break;
                
            default:
                return BadRequest($"Unknown scenario: {scenarioName}. Available scenarios: underload, nearcapacity, overload, lowvariability, highservicevariability, highvariability");
        }
        
        _configService.UpdateConfiguration(config);
        
        _logger.LogInformation(
            "Scenario {ScenarioName} applied: OrderArrivalRateMs={OrderArrivalRateMs}, BillingProcessingDelayMs={BillingProcessingDelayMs}, BillingServiceCount={BillingServiceCount}, Ca={Ca}, Cs={Cs}",
            scenarioName,
            config.OrderArrivalDelayMs,
            config.BillingProcessingDelayMs,
            config.BillingServiceCount,
            config.CoefficientOfArrivalVariation,
            config.CoefficientOfServiceVariation);
            
        return Ok(_configService.GetConfiguration());
    }
}
