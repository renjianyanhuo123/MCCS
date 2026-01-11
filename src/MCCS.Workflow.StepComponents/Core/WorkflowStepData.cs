using System.Text.Json;

namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 工作流步骤数据 - 在整个工作流中传递的数据容器
    /// 这是 WorkflowCore 工作流的数据类
    /// </summary>
    public class WorkflowStepData
    {
        /// <summary>
        /// 工作流实例ID
        /// </summary>
        public string WorkflowInstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 工作流定义ID
        /// </summary>
        public string WorkflowDefinitionId { get; set; } = string.Empty;

        /// <summary>
        /// 全局变量存储
        /// </summary>
        public Dictionary<string, object?> Variables { get; set; } = new();

        /// <summary>
        /// 步骤输出结果存储（按步骤ID索引）
        /// </summary>
        public Dictionary<string, StepOutputData> StepOutputs { get; set; } = new();

        /// <summary>
        /// 步骤配置参数存储（按步骤ID索引）
        /// </summary>
        public Dictionary<string, Dictionary<string, object?>> StepConfigs { get; set; } = new();

        /// <summary>
        /// 最后一个步骤的输出
        /// </summary>
        public StepOutputData? LastStepOutput { get; set; }

        /// <summary>
        /// 工作流开始时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 工作流状态
        /// </summary>
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Running;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        #region 便捷方法

        /// <summary>
        /// 获取变量
        /// </summary>
        public T? GetVariable<T>(string name)
        {
            if (Variables.TryGetValue(name, out var value))
            {
                return ConvertValue<T>(value);
            }
            return default;
        }

        /// <summary>
        /// 设置变量
        /// </summary>
        public void SetVariable(string name, object? value)
        {
            Variables[name] = value;
        }

        /// <summary>
        /// 获取步骤输出
        /// </summary>
        public StepOutputData? GetStepOutput(string stepId) => StepOutputs.GetValueOrDefault(stepId);

        /// <summary>
        /// 设置步骤输出
        /// </summary>
        public void SetStepOutput(string stepId, StepOutputData output)
        {
            StepOutputs[stepId] = output;
            LastStepOutput = output;
        }

        /// <summary>
        /// 获取步骤配置
        /// </summary>
        public Dictionary<string, object?>? GetStepConfig(string stepId)
        {
            return StepConfigs.TryGetValue(stepId, out var config) ? config : null;
        }

        /// <summary>
        /// 设置步骤配置
        /// </summary>
        public void SetStepConfig(string stepId, Dictionary<string, object?> config)
        {
            StepConfigs[stepId] = config;
        } 

        /// <summary>
        /// 替换字符串中的变量引用
        /// </summary>
        public string ReplaceVariables(string template)
        {
            if (string.IsNullOrEmpty(template)) return template;

            var result = template;

            // 替换变量 ${variableName}
            foreach (var kvp in Variables)
            {
                result = result.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            // 替换上一步输出 ${prev.propertyName}
            if (LastStepOutput != null)
            {
                foreach (var kvp in LastStepOutput.Data)
                {
                    result = result.Replace($"${{prev.{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
                }
            }

            return result;
        }

        private static T? ConvertValue<T>(object? value)
        {
            if (value == null) return default;
            if (value is T typed) return typed;

            try
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        #endregion
    }

    /// <summary>
    /// 步骤输出数据
    /// </summary>
    public class StepOutputData
    {
        /// <summary>
        /// 步骤ID
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 执行状态
        /// </summary>
        public StepExecutionStatus Status { get; set; }

        /// <summary>
        /// 输出数据
        /// </summary>
        public Dictionary<string, object?> Data { get; set; } = new();

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 工作流状态
    /// </summary>
    public enum WorkflowStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Suspended,
        Cancelled
    }

    /// <summary>
    /// 步骤执行状态
    /// </summary>
    public enum StepExecutionStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Skipped,
        Cancelled
    }
}
