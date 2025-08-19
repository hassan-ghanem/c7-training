using Camunda.Api.Client;
using Camunda.Api.Client.ProcessDefinition;
using Camunda.Api.Client.UserTask;
using Microsoft.Extensions.Logging;

namespace CamundaLearningApp;

public class CamundaService
{
    private readonly CamundaClient _camundaClient;
    private readonly ILogger<CamundaService> _logger;

    public CamundaService(CamundaClient camundaClient, ILogger<CamundaService> logger)
    {
        _camundaClient = camundaClient;
        _logger = logger;
    }

    public async Task RunExamplesAsync()
    {
        _logger.LogInformation("=== Camunda Learning Examples ===\n");

        try
        {
            // 1. List available process definitions
            await ListProcessDefinitionsAsync();

            // 2. Start a process instance (if any process definitions exist)
            var processInstanceId = await StartProcessInstanceAsync("test_process");

            // 3. List tasks for a specific assignee
            await ListTasksForAssigneeAsync("demo");

            // 4. List all active tasks
            await ListAllActiveTasksAsync();

            // 5. Complete a task (if any tasks exist)
            await CompleteTaskAsync();

            // 6. Show process instances
            await ListProcessInstancesAsync();
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

    private async Task<string?> StartProcessInstanceAsync(string processDefinitionKey)
    {
        try
        {
            _logger.LogInformation("2. Starting a Process Instance:");
            _logger.LogInformation("===============================");

            // Get the process definition by key
            var processDefinitions = await _camundaClient.ProcessDefinitions.Query().List();
            var processDefinition = processDefinitions.FirstOrDefault(pd => pd.Key == processDefinitionKey);

            if (processDefinition == null)
            {
                _logger.LogWarning($"  No process definition found with key '{processDefinitionKey}'.");
                _logger.LogInformation("  To see this example in action:");
                _logger.LogInformation($"  1. Deploy a BPMN process with key '{processDefinitionKey}' to Camunda");
                _logger.LogInformation("  2. Run this application again");
                return null;
            }

            // Create start process instance request
            var startRequest = new StartProcessInstance
            {
                Variables = new Dictionary<string, VariableValue>
                {
                    ["customerName"] = VariableValue.FromObject("John Doe"),
                    ["orderAmount"] = VariableValue.FromObject(150.75),
                    ["priority"] = VariableValue.FromObject("high")
                },
                BusinessKey = $"order-{DateTime.Now:yyyyMMdd-HHmmss}"
            };

            var processInstance = await _camundaClient.ProcessDefinitions.ByKey(processDefinitionKey).StartProcessInstance(startRequest);

            _logger.LogInformation($"  ✓ Started process instance: {processInstance.Id}");
            _logger.LogInformation($"  Process Definition: {processDefinition.Key} (v{processDefinition.Version})");
            _logger.LogInformation($"  Business Key: {startRequest.BusinessKey}");
            _logger.LogInformation($"  Variables set: {string.Join(", ", startRequest.Variables.Keys)}");

            return processInstance.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting process instance: {ex.Message}");
            return null;
        }
        finally
        {
            _logger.LogInformation("");
        }
    }

    private async Task ListTasksForAssigneeAsync(string assignee)
    {
        try
        {
            _logger.LogInformation($"3. Listing Tasks for Assignee '{assignee}':");
            _logger.LogInformation("==========================================");

            var taskQuery = new TaskQuery { Assignee = assignee };
            var tasks = await _camundaClient.UserTasks.Query(taskQuery).List();

            if (tasks.Any())
            {
                foreach (var task in tasks)
                {
                    _logger.LogInformation($"  - Task ID: {task.Id}");
                    _logger.LogInformation($"    Name: {task.Name ?? "N/A"}");
                    _logger.LogInformation($"    Assignee: {task.Assignee ?? "N/A"}");
                    _logger.LogInformation($"    Created: {task.Created:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation($"    Process Instance: {task.ProcessInstanceId}");
                    _logger.LogInformation("");
                }
            }
            else
            {
                _logger.LogInformation($"  No tasks found for assignee '{assignee}'");
                _logger.LogInformation("  Try creating tasks assigned to 'demo' user or change the assignee parameter");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error listing tasks for assignee: {ex.Message}");
        }

        _logger.LogInformation("");
    }

    private async Task ListAllActiveTasksAsync()
    {
        try
        {
            _logger.LogInformation("4. Listing All Active Tasks:");
            _logger.LogInformation("============================");

            var tasks = await _camundaClient.UserTasks.Query().List();

            if (tasks.Any())
            {
                foreach (var task in tasks)
                {
                    _logger.LogInformation($"  - Task ID: {task.Id}");
                    _logger.LogInformation($"    Name: {task.Name ?? "N/A"}");
                    _logger.LogInformation($"    Assignee: {task.Assignee ?? "Unassigned"}");
                    _logger.LogInformation($"    Task Key: {task.TaskDefinitionKey ?? "N/A"}");
                    _logger.LogInformation($"    Created: {task.Created:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation($"    Due Date: {task.Due?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
                    _logger.LogInformation($"    Priority: {task.Priority}");
                    _logger.LogInformation("");
                }
            }
            else
            {
                _logger.LogInformation("  No active tasks found.");
                _logger.LogInformation("  Start a process instance with user tasks to see examples here.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error listing all tasks: {ex.Message}");
        }

        _logger.LogInformation("");
    }

    private async Task CompleteTaskAsync()
    {
        try
        {
            _logger.LogInformation("5. Completing a Task:");
            _logger.LogInformation("====================");

            // Get the first available task
            var tasks = await _camundaClient.UserTasks.Query().List();
            var task = tasks.FirstOrDefault();

            if (task == null)
            {
                _logger.LogInformation("  No tasks available to complete.");
                _logger.LogInformation("  Start a process with user tasks to see this example in action.");
                return;
            }

            // Complete the task with some variables
            var completionVariables = new Dictionary<string, VariableValue>
            {
                ["approved"] = VariableValue.FromObject(true),
                ["comments"] = VariableValue.FromObject("Approved via API example"),
                ["completedBy"] = VariableValue.FromObject("CamundaLearningApp"),
                ["completedAt"] = VariableValue.FromObject(DateTime.Now)
            };

            var completeRequest = new CompleteTask
            {
                Variables = completionVariables
            };

            await _camundaClient.UserTasks[task.Id].Complete(completeRequest);

            _logger.LogInformation($"  ✓ Completed task: {task.Id}");
            _logger.LogInformation($"  Task Name: {task.Name ?? "N/A"}");
            _logger.LogInformation($"  Variables set: {string.Join(", ", completionVariables.Keys)}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error completing task: {ex.Message}");
        }

        _logger.LogInformation("");
    }

    private async Task ListProcessInstancesAsync()
    {
        try
        {
            _logger.LogInformation("6. Listing Process Instances:");
            _logger.LogInformation("=============================");

            var processInstances = await _camundaClient.ProcessInstances.Query().List();

            if (processInstances.Any())
            {
                foreach (var pi in processInstances)
                {
                    _logger.LogInformation($"  - Instance ID: {pi.Id}");
                    _logger.LogInformation($"    Definition ID: {pi.DefinitionId}");
                    _logger.LogInformation($"    Business Key: {pi.BusinessKey ?? "N/A"}");
                    _logger.LogInformation($"    Suspended: {pi.Suspended}");
                    _logger.LogInformation("");
                }
            }
            else
            {
                _logger.LogInformation("  No process instances found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error listing process instances: {ex.Message}");
        }

        _logger.LogInformation("");
    }
}
