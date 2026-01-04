using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Registry;
using WorkflowCore.Interface;

namespace MCCS.Workflow.StepComponents.Workflows
{
    /// <summary>
    /// 动态工作流 - 根据配置动态构建工作流
    /// </summary>
    public class DynamicWorkflow : IWorkflow<WorkflowStepData>
    {
        private readonly WorkflowDefinition _definition;
        private readonly IStepRegistry _stepRegistry;

        public string Id => _definition.Id;
        public int Version => _definition.Version;

        public DynamicWorkflow(WorkflowDefinition definition, IStepRegistry stepRegistry)
        {
            _definition = definition;
            _stepRegistry = stepRegistry;
        }

        public void Build(IWorkflowBuilder<WorkflowStepData> builder)
        {
            if (_definition.Steps.Count == 0)
            {
                return;
            }

            // 构建步骤链
            IStepBuilder<WorkflowStepData, BaseWorkflowStep>? currentStep = null;

            for (int i = 0; i < _definition.Steps.Count; i++)
            {
                var stepConfig = _definition.Steps[i];
                var stepType = _stepRegistry.GetStepType(stepConfig.StepTypeId);

                if (stepType == null)
                {
                    throw new InvalidOperationException($"未找到步骤类型: {stepConfig.StepTypeId}");
                }

                if (i == 0)
                {
                    // 第一个步骤
                    //currentStep = builder
                    //    .StartWith(stepType, x =>
                    //    {
                    //        ConfigureStep(x, stepConfig);
                    //    })
                    //    .Name(stepConfig.Name)
                    //    .Id(stepConfig.Id);
                }
                else if (currentStep != null)
                {
                    // 后续步骤
                    //currentStep = currentStep
                    //    .Then(stepType, x =>
                    //    {
                    //        ConfigureStep(x, stepConfig);
                    //    })
                    //    .Name(stepConfig.Name)
                    //    .Id(stepConfig.Id);
                }
            }
        }

        private static void ConfigureStep(IStepBuilder<WorkflowStepData, BaseWorkflowStep> stepBuilder, StepConfiguration config)
        {
            // 设置步骤ID
            stepBuilder.Input(step => step.CurrentStepId, data => config.Id);

            // 设置步骤配置
            stepBuilder.Input(step => step.StepConfig, data => config.Parameters);

            // 设置输出映射
            stepBuilder.Output(data => data.LastStepOutput, step => step.StepOutput);
        }
    }

    /// <summary>
    /// 工作流定义
    /// </summary>
    public class WorkflowDefinition
    {
        /// <summary>
        /// 工作流ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 工作流描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 工作流版本
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// 步骤配置列表
        /// </summary>
        public List<StepConfiguration> Steps { get; set; } = new();

        /// <summary>
        /// 初始变量
        /// </summary>
        public Dictionary<string, object?> InitialVariables { get; set; } = new();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 步骤配置
    /// </summary>
    public class StepConfiguration
    {
        /// <summary>
        /// 步骤实例ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 步骤类型ID（对应注册的步骤ID）
        /// </summary>
        public string StepTypeId { get; set; } = string.Empty;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 步骤参数
        /// </summary>
        public Dictionary<string, object?> Parameters { get; set; } = new();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 错误处理策略
        /// </summary>
        public ErrorHandlingStrategy ErrorHandling { get; set; } = ErrorHandlingStrategy.Fail;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 重试间隔（秒）
        /// </summary>
        public int RetryInterval { get; set; } = 5;
    }

    /// <summary>
    /// 错误处理策略
    /// </summary>
    public enum ErrorHandlingStrategy
    {
        /// <summary>
        /// 失败时停止工作流
        /// </summary>
        Fail,

        /// <summary>
        /// 失败时继续执行下一步
        /// </summary>
        Continue,

        /// <summary>
        /// 失败时重试
        /// </summary>
        Retry,

        /// <summary>
        /// 失败时挂起等待人工处理
        /// </summary>
        Suspend
    }
}
