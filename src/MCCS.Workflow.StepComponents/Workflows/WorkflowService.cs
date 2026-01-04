using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Registry;
using WorkflowCore.Interface;
using WorkflowCore.Models;

using WorkflowStatus = MCCS.Workflow.StepComponents.Core.WorkflowStatus;

namespace MCCS.Workflow.StepComponents.Workflows
{
    /// <summary>
    /// 工作流服务实现
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowHost _workflowHost;
        private readonly IStepRegistry _stepRegistry;
        private readonly Dictionary<string, WorkflowDefinition> _definitions = new();
        private readonly object _lock = new();

        public event EventHandler<WorkflowCompletedEventArgs>? WorkflowCompleted;
        public event EventHandler<StepCompletedEventArgs>? StepCompleted;
        public event EventHandler<WorkflowErrorEventArgs>? WorkflowError;

        public WorkflowService(IWorkflowHost workflowHost, IStepRegistry stepRegistry)
        {
            _workflowHost = workflowHost;
            _stepRegistry = stepRegistry;

            // 订阅工作流事件
            _workflowHost.OnStepError += OnStepError;
        }

        public void RegisterWorkflow(WorkflowDefinition definition)
        {
            lock (_lock)
            {
                _definitions[definition.Id] = definition;

                // 创建并注册动态工作流
                var dynamicWorkflow = new DynamicWorkflow(definition, _stepRegistry);
                _workflowHost.Registry.RegisterWorkflow(dynamicWorkflow);
            }
        }

        public async Task<string> StartWorkflowAsync(string workflowId, WorkflowStepData? initialData = null)
        {
            WorkflowDefinition? definition;
            lock (_lock)
            {
                if (!_definitions.TryGetValue(workflowId, out definition))
                {
                    throw new InvalidOperationException($"未找到工作流定义: {workflowId}");
                }
            }

            return await StartWorkflowAsync(definition, initialData);
        }

        public async Task<string> StartWorkflowAsync(WorkflowDefinition definition, WorkflowStepData? initialData = null)
        {
            // 确保工作流已注册
            lock (_lock)
            {
                if (!_definitions.ContainsKey(definition.Id))
                {
                    RegisterWorkflow(definition);
                }
            }

            // 准备初始数据
            var data = initialData ?? new WorkflowStepData();
            data.WorkflowDefinitionId = definition.Id;
            data.StartTime = DateTime.Now;
            data.Status = WorkflowStatus.Running;

            // 加载初始变量
            foreach (var kvp in definition.InitialVariables)
            {
                data.SetVariable(kvp.Key, kvp.Value);
            }

            // 加载步骤配置
            foreach (var step in definition.Steps)
            {
                data.SetStepConfig(step.Id, step.Parameters);
            }

            // 启动工作流
            var instanceId = await _workflowHost.StartWorkflow(definition.Id, definition.Version, data);
            data.WorkflowInstanceId = instanceId;

            return instanceId;
        }

        public async Task<bool> SuspendWorkflowAsync(string workflowInstanceId)
        {
            return await _workflowHost.SuspendWorkflow(workflowInstanceId);
        }

        public async Task<bool> ResumeWorkflowAsync(string workflowInstanceId)
        {
            return await _workflowHost.ResumeWorkflow(workflowInstanceId);
        }

        public async Task<bool> TerminateWorkflowAsync(string workflowInstanceId)
        {
            return await _workflowHost.TerminateWorkflow(workflowInstanceId);
        }

        public async Task<WorkflowInstanceInfo?> GetWorkflowStatusAsync(string workflowInstanceId)
        {
            var instance = await _workflowHost.PersistenceStore.GetWorkflowInstance(workflowInstanceId);
            if (instance == null) return null;

            return new WorkflowInstanceInfo
            {
                InstanceId = instance.Id,
                WorkflowId = instance.WorkflowDefinitionId,
                Version = instance.Version,
                Status = MapWorkflowStatus(instance.Status),
                CreateTime = instance.CreateTime,
                CompleteTime = instance.CompleteTime,
                Data = instance.Data as WorkflowStepData
            };
        }

        public async Task PublishEventAsync(string eventName, string eventKey, object? eventData = null)
        {
            await _workflowHost.PublishEvent(eventName, eventKey, eventData);
        }

        private void OnStepError(WorkflowInstance workflow, WorkflowStep step, Exception exception)
        {
            WorkflowError?.Invoke(this, new WorkflowErrorEventArgs
            {
                WorkflowInstanceId = workflow.Id,
                StepId = step.Id.ToString(),
                ErrorMessage = exception.Message,
                Exception = exception
            });
        }

        private static WorkflowStatus MapWorkflowStatus(WorkflowCore.Models.WorkflowStatus status)
        {
            return status switch
            {
                WorkflowCore.Models.WorkflowStatus.Runnable => WorkflowStatus.Running,
                WorkflowCore.Models.WorkflowStatus.Suspended => WorkflowStatus.Suspended,
                WorkflowCore.Models.WorkflowStatus.Complete => WorkflowStatus.Completed,
                WorkflowCore.Models.WorkflowStatus.Terminated => WorkflowStatus.Cancelled,
                _ => WorkflowStatus.Pending
            };
        }
    }
}
