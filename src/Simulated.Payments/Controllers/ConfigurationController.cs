using Microsoft.AspNetCore.Mvc;
using Simulated.Payments.Models;
using Simulated.Payments.Services;

namespace Simulated.Payments.Controllers;

[ApiController]
[Route("api/payment/configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly PaymentConfigurationService _configService;

    public ConfigurationController(
        ILogger<ConfigurationController> logger,
        PaymentConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
    }

    [HttpGet]
    public ActionResult<PaymentConfiguration> GetConfiguration()
    {
        try
        {
            var config = _configService.GetConfiguration();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration");
            return StatusCode(500, "An error occurred while retrieving the configuration");
        }
    }

    [HttpPut]
    public ActionResult<PaymentConfiguration> UpdateConfiguration([FromBody] PaymentConfiguration config)
    {
        if (config == null)
        {
            return BadRequest("Configuration cannot be null");
        }

        if (config.ProcessingDelayMs < 0)
        {
            return BadRequest("Processing delay must be a non-negative value");
        }

        try
        {
            var updatedConfig = _configService.UpdateConfiguration(config);
            _logger.LogInformation("Configuration updated: ProcessingDelayMs = {ProcessingDelayMs}", updatedConfig.ProcessingDelayMs);
            return Ok(updatedConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration");
            return StatusCode(500, "An error occurred while updating the configuration");
        }
    }
}