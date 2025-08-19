using Camunda.Api.Client;
using Microsoft.Extensions.Logging;

namespace CamundaLearningApp;

public class SimpleCamundaService
{
    private readonly CamundaClient _camundaClient;
    private readonly ILogger<SimpleCamundaService> _logger;

    public SimpleCamundaService(CamundaClient camundaClient, ILogger<SimpleCamundaService> logger)
    {
        _camundaClient = camundaClient;
        _logger = logger;
    }

    public async Task RunExamplesAsync()
    {
        _logger.LogInformation("=== Simple Camunda Learning Examples ===\n");

        try
        {
            // 1. List available process definitions
            await ListProcessDefinitionsAsync();

            // 2. Show what API endpoints are available
            await ShowAvailableAPIsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during examples: {ex.Message}");
            _logger.LogInformation("This might be due to API differences. Please check the Camunda.Api.Client documentation.");
        }
    }

    private async Task ListProcessDefinitionsAsync()
    {
        try
        {
            _logger.LogInformation("1. Listing Process Definitions:");
            _logger.LogInformation("================================");

            var processDefinitions = await _camundaClient.ProcessDefinitions.Query().List();

            if (processDefinitions.Any())
            {
                foreach (var pd in processDefinitions)
                {
                    _logger.LogInformation($"  - ID: {pd.Id}");
                    _logger.LogInformation($"    Key: {pd.Key}");
                    _logger.LogInformation($"    Name: {pd.Name ?? "N/A"}");
                    _logger.LogInformation($"    Version: {pd.Version}");
                    _logger.LogInformation($"    Deployment ID: {pd.DeploymentId}");
                    _logger.LogInformation("");
                }
            }
            else
            {
                _logger.LogWarning("  No process definitions found. Deploy a BPMN process to see examples.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error listing process definitions: {ex.Message}");
        }

        _logger.LogInformation("");
    }

    private async Task ShowAvailableAPIsAsync()
    {
        try
        {
            _logger.LogInformation("2. Available Camunda API Endpoints:");
            _logger.LogInformation("====================================");

            // Show what's available in the client
            var clientType = _camundaClient.GetType();
            var properties = clientType.GetProperties();

            _logger.LogInformation("Available API endpoints:");
            foreach (var prop in properties)
            {
                _logger.LogInformation($"  - {prop.Name}: {prop.PropertyType.Name}");
            }

            _logger.LogInformation("");
            _logger.LogInformation("To use these APIs, check the Camunda.Api.Client documentation:");
            _logger.LogInformation("https://github.com/jlucansky/Camunda.Api.Client");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error showing available APIs: {ex.Message}");
        }

        _logger.LogInformation("");
    }
}
