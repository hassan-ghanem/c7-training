namespace DemoTasklist.Services
{
    using Camunda.Api.Client;
    using Camunda.Api.Client.History;
    using Camunda.Api.Client.Message;
    using Camunda.Api.Client.ProcessDefinition;
    using Camunda.Api.Client.UserTask;
    using DemoTasklist.Models;
    using System.Collections.Generic;

    using System.Threading.Tasks;

    public class CamundaService
    {
        private CamundaClient _camundaClient;

        public CamundaService(string camundaApiUrl)
        {
            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri(camundaApiUrl)
            };

            _camundaClient = CamundaClient.Create(httpClient);
        }

        public async Task<List<UserTaskInfo>> GetTasks(TaskQuery? userTaskQuery)
        {
            return await _camundaClient.UserTasks.Query(userTaskQuery).List();
        }

        public async Task<List<UserTaskInfo>> GetTasks(TaskQuery? userTaskQuery, int firstResult, int maxResults)
        {
            return await _camundaClient.UserTasks.Query(userTaskQuery).List(firstResult, maxResults);
        }

        public async Task<List<UserTaskInfo>> GetTasks()
        {
            return await GetTasks(null);
        }

        public async Task<List<UserTaskInfo>> GetTasks(int firstResult, int maxResults)
        {
            return await GetTasks(null, firstResult, maxResults);
        }

        public async Task<int> GetTasksCount(TaskQuery? userTaskQuery)
        {
            return await _camundaClient.UserTasks.Query(userTaskQuery).Count();
        }

        public async Task<UserTaskInfo> GetTaskById(string taskId)
        {
            return await _camundaClient.UserTasks[taskId].Get();
        }

        public async Task<Dictionary<string, VariableValue>> GetTaskVariables(string taskId)
        {
            return await _camundaClient.UserTasks[taskId].Variables.GetAll();
        }

        public async Task<HttpContent> GetBinaryVariable(string taskId, string variableName)
        {
            return await _camundaClient.UserTasks[taskId].Variables.GetBinary(variableName);
        }

        public async Task CompleteTask(string taskId, Dictionary<string, object>? variables)
        {
            var completeTask = new CompleteTask
            {
                Variables = []
            };

            if (variables != null)
            {
                foreach (var variable in variables)
                {
                    VariableValue vv = VariableValue.FromObject(variable.Value);

                    completeTask.Variables.Add(variable.Key, vv);
                }
            }

            await _camundaClient.UserTasks[taskId].Complete(completeTask);
        }

        public async Task CompleteTask(string taskId, string action, string? comment,
            Dictionary<string, object>? variables)
        {

            // Set the "action" local variable
            await _camundaClient.UserTasks[taskId].LocalVariables.Set("sys_action", VariableValue.FromObject(action));
            // Set the "comment" local variable
            await _camundaClient.UserTasks[taskId].LocalVariables.Set("sys_comment", VariableValue.FromObject(comment));

            await CompleteTask(taskId, variables);
        }


        public async Task CompleteTask(string taskId)
        {
            await CompleteTask(taskId, null);
        }

        public async Task<string?> StartProcessInstance(string processDefinitionKey, Dictionary<string, object>? variables, string? businessKey = null)
        {
            var startInstance = new StartProcessInstance
            {
                Variables = []
            };

            if (!string.IsNullOrEmpty(businessKey))
            {
                startInstance.BusinessKey = businessKey;
            }

            if (variables != null)
            {
                foreach (var variable in variables)
                {
                    VariableValue vv = VariableValue.FromObject(variable.Value);

                    startInstance.Variables.Add(variable.Key, vv);
                }
            }

            var processStartResult = await _camundaClient.ProcessDefinitions.ByKey(processDefinitionKey).StartProcessInstance(startInstance);

            return processStartResult?.Id;
        }

        public async Task CorrelateMessage(string messageName, Dictionary<string, object>? processVariables, string? businessKey = null)
        {
            var correlationMessage = new CorrelationMessage
            {
                MessageName = messageName,
                BusinessKey = businessKey,
                ProcessVariables = []
            };

            if (processVariables != null)
            {
                foreach (var variable in processVariables)
                {
                    VariableValue vv = VariableValue.FromObject(variable.Value);
                    correlationMessage.ProcessVariables.Add(variable.Key, vv);
                }
            }

            await _camundaClient.Messages.DeliverMessage(correlationMessage);
        }

        public async Task<List<HistoricActivityInstance>> GetHistoricalActivityInstances(HistoricActivityInstanceQuery query)
        {
            return await _camundaClient.History.ActivityInstances.Query(query).List();
        }

        public async Task<List<HistoricVariableInstance>> GetHistoricalVariableInstances(HistoricVariableInstanceQuery query)
        {
            return await _camundaClient.History.VariableInstances.Query(query).List();
        }

        public async Task<List<HistoricProcessInstance>> GetHistoricalProcessInstances(HistoricProcessInstanceQuery query)
        {
            return await _camundaClient.History.ProcessInstances.Query(query).List();
        }

        public async Task<List<HistoricProcessInstance>> GetHistoricalProcessInstances(HistoricProcessInstanceQuery query, int firstResult, int maxResults)
        {
            return await _camundaClient.History.ProcessInstances.Query(query).List(firstResult, maxResults);
        }

        public async Task<int> GetHistoricProcessInstancesCount(HistoricProcessInstanceQuery query)
        {
            return await _camundaClient.History.ProcessInstances.Query(query).Count();
        }

        public async Task<string> GetBpmnXml(string processDefinitionKey)
        {
            var processDefinition = await _camundaClient.ProcessDefinitions.ByKey(processDefinitionKey).GetXml();

            return processDefinition.Bpmn20Xml;
        }
    }
}
