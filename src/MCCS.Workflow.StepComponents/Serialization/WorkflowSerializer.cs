using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Workflows;

namespace MCCS.Workflow.StepComponents.Serialization
{
    /// <summary>
    /// 工作流序列化器实现
    /// </summary>
    public class WorkflowSerializer : IWorkflowSerializer
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public WorkflowSerializer()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public string SerializeWorkflow(WorkflowDefinition definition)
        {
            return JsonSerializer.Serialize(definition, _jsonOptions);
        }

        public WorkflowDefinition? DeserializeWorkflow(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<WorkflowDefinition>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public string SerializeStep(StepConfiguration step)
        {
            return JsonSerializer.Serialize(step, _jsonOptions);
        }

        public StepConfiguration? DeserializeStep(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<StepConfiguration>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public string SerializeWorkflowData(WorkflowStepData data)
        {
            return JsonSerializer.Serialize(data, _jsonOptions);
        }

        public WorkflowStepData? DeserializeWorkflowData(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<WorkflowStepData>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveWorkflowAsync(WorkflowDefinition definition, string filePath)
        {
            var json = SerializeWorkflow(definition);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<WorkflowDefinition?> LoadWorkflowAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            return DeserializeWorkflow(json);
        }
    }
}
