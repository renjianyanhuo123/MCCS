using System.Diagnostics;
using System.Reflection;
using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Parameters;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 工作流步骤基类 - 基于 WorkflowCore 的 StepBodyAsync
    /// 所有自定义步骤组件都应继承此类
    /// </summary>
    public abstract class BaseWorkflowStep : StepBodyAsync
    {
        private readonly StepComponentAttribute? _componentAttribute;
        private List<IComponentParameter>? _parameters;

        protected BaseWorkflowStep()
        {
            _componentAttribute = GetType().GetCustomAttribute<StepComponentAttribute>();
        }

        #region 元数据属性

        /// <summary>
        /// 步骤唯一标识
        /// </summary>
        public virtual string StepId => _componentAttribute?.Id ?? GetType().Name;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public virtual string StepName => _componentAttribute?.Name ?? GetType().Name;

        /// <summary>
        /// 步骤描述
        /// </summary>
        public virtual string Description => _componentAttribute?.Description ?? string.Empty;

        /// <summary>
        /// 步骤分类
        /// </summary>
        public virtual ComponentCategory Category => _componentAttribute?.Category ?? ComponentCategory.General;

        /// <summary>
        /// 步骤图标
        /// </summary>
        public virtual string Icon => _componentAttribute?.Icon ?? "Cog";

        /// <summary>
        /// 步骤版本
        /// </summary>
        public virtual string Version => _componentAttribute?.Version ?? "1.0.0";

        #endregion

        #region WorkflowCore 输入/输出属性

        /// <summary>
        /// 当前步骤实例ID（由工作流引擎设置）
        /// </summary>
        public string? CurrentStepId { get; set; }

        /// <summary>
        /// 步骤配置参数（从工作流数据映射）
        /// </summary>
        public Dictionary<string, object?>? StepConfig { get; set; }

        /// <summary>
        /// 步骤输出结果
        /// </summary>
        public StepOutputData? StepOutput { get; set; }

        #endregion

        #region StepBodyAsync 实现

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var workflowData = context.Workflow.Data as WorkflowStepData;

            // 初始化参数
            InitializeParameters();

            // 从配置加载参数值
            if (StepConfig != null)
            {
                LoadParametersFromConfig(StepConfig);
            }
            else if (workflowData != null && CurrentStepId != null)
            {
                var config = workflowData.GetStepConfig(CurrentStepId);
                if (config != null)
                {
                    LoadParametersFromConfig(config);
                }
            }

            try
            {
                // 验证参数
                var validationResult = ValidateParameters();
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException(
                        $"步骤参数验证失败: {string.Join("; ", validationResult.Errors.Select(e => e.Message))}");
                }

                // 创建步骤执行上下文
                var stepContext = new StepExecutionContext
                {
                    WorkflowContext = context,
                    WorkflowData = workflowData ?? new WorkflowStepData(),
                    StepId = CurrentStepId ?? context.Step.Id.ToString(),
                    StepName = StepName,
                    CancellationToken = context.CancellationToken
                };

                // 执行前回调
                await OnBeforeExecuteAsync(stepContext);

                // 执行核心逻辑
                var result = await ExecuteAsync(stepContext);

                stopwatch.Stop();

                // 创建输出数据
                StepOutput = new StepOutputData
                {
                    StepId = stepContext.StepId,
                    StepName = StepName,
                    Status = result.Success ? StepExecutionStatus.Completed : StepExecutionStatus.Failed,
                    Data = result.OutputData,
                    ErrorMessage = result.ErrorMessage,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                // 保存到工作流数据
                workflowData?.SetStepOutput(stepContext.StepId, StepOutput);
                workflowData?.AddLog($"步骤 [{StepName}] 执行完成，耗时 {stopwatch.ElapsedMilliseconds}ms",
                    result.Success ? LogLevel.Info : LogLevel.Error, stepContext.StepId);

                // 执行后回调
                await OnAfterExecuteAsync(stepContext, result);

                // 处理下一步决策
                if (result.NextStepName != null)
                {
                    return ExecutionResult.Branch(new List<string> { result.NextStepName }, result.OutputData);
                }

                if (result.ProceedToNext)
                {
                    return ExecutionResult.Next();
                }

                return ExecutionResult.Persist(result.OutputData);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                StepOutput = new StepOutputData
                {
                    StepId = CurrentStepId ?? context.Step.Id.ToString(),
                    StepName = StepName,
                    Status = StepExecutionStatus.Cancelled,
                    ErrorMessage = "步骤执行被取消",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                workflowData?.SetStepOutput(CurrentStepId ?? context.Step.Id.ToString(), StepOutput);
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StepOutput = new StepOutputData
                {
                    StepId = CurrentStepId ?? context.Step.Id.ToString(),
                    StepName = StepName,
                    Status = StepExecutionStatus.Failed,
                    ErrorMessage = ex.Message,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                workflowData?.SetStepOutput(CurrentStepId ?? context.Step.Id.ToString(), StepOutput);
                workflowData?.AddLog($"步骤 [{StepName}] 执行失败: {ex.Message}", LogLevel.Error,
                    CurrentStepId ?? context.Step.Id.ToString());

                throw;
            }
        }

        #endregion

        #region 抽象方法 - 子类必须实现

        /// <summary>
        /// 执行步骤核心逻辑
        /// </summary>
        protected abstract Task<StepResult> ExecuteAsync(StepExecutionContext context);

        /// <summary>
        /// 定义步骤参数
        /// </summary>
        protected abstract IEnumerable<IComponentParameter> DefineParameters();

        #endregion

        #region 可重写的回调方法

        /// <summary>
        /// 执行前回调
        /// </summary>
        protected virtual Task OnBeforeExecuteAsync(StepExecutionContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 执行后回调
        /// </summary>
        protected virtual Task OnAfterExecuteAsync(StepExecutionContext context, StepResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        protected virtual ComponentValidationResult ValidateCustom()
        {
            return ComponentValidationResult.Valid();
        }

        #endregion

        #region 参数管理

        /// <summary>
        /// 获取参数定义列表
        /// </summary>
        public IReadOnlyList<IComponentParameter> GetParameterDefinitions()
        {
            InitializeParameters();
            return _parameters!.AsReadOnly();
        }

        /// <summary>
        /// 获取所有参数值
        /// </summary>
        public Dictionary<string, object?> GetParameterValues()
        {
            InitializeParameters();
            return _parameters!.ToDictionary(p => p.Name, p => p.Value);
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        public void SetParameterValues(Dictionary<string, object?> values)
        {
            InitializeParameters();
            LoadParametersFromConfig(values);
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        protected T? GetParameter<T>(string name)
        {
            InitializeParameters();
            var param = _parameters!.FirstOrDefault(p => p.Name == name);
            if (param?.Value == null) return default;

            if (param.Value is T typed) return typed;

            try
            {
                return (T)Convert.ChangeType(param.Value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        protected void SetParameter(string name, object? value)
        {
            InitializeParameters();
            var param = _parameters!.FirstOrDefault(p => p.Name == name);
            if (param != null)
            {
                param.Value = value;
            }
        }

        private void InitializeParameters()
        {
            if (_parameters != null) return;

            _parameters = DefineParameters().ToList();

            // 设置默认值
            foreach (var param in _parameters)
            {
                if (param.Value == null && param.DefaultValue != null)
                {
                    param.Value = param.DefaultValue;
                }
            }
        }

        private void LoadParametersFromConfig(Dictionary<string, object?> config)
        {
            InitializeParameters();

            foreach (var kvp in config)
            {
                var param = _parameters!.FirstOrDefault(p => p.Name == kvp.Key);
                if (param != null)
                {
                    param.Value = kvp.Value;
                }
            }

            // 同步到属性
            SyncParametersToProperties();
        }

        private void SyncParametersToProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var param in _parameters!)
            {
                var prop = properties.FirstOrDefault(p =>
                    p.GetCustomAttribute<StepInputAttribute>()?.Name == param.Name ||
                    p.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase));

                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        var value = ConvertValue(param.Value, prop.PropertyType);
                        prop.SetValue(this, value);
                    }
                    catch
                    {
                        // 忽略转换错误
                    }
                }
            }
        }

        private ComponentValidationResult ValidateParameters()
        {
            var result = new ComponentValidationResult { IsValid = true };

            foreach (var param in _parameters!)
            {
                var paramResult = param.Validate();
                if (!paramResult.IsValid)
                {
                    result.AddError(param.Name, paramResult.ErrorMessage ?? "验证失败");
                }
            }

            var customResult = ValidateCustom();
            if (!customResult.IsValid)
            {
                foreach (var error in customResult.Errors)
                {
                    result.AddError(error.ParameterName, error.Message);
                }
            }

            return result;
        }

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null) return null;

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (value.GetType() == underlyingType)
            {
                return value;
            }

            return Convert.ChangeType(value, underlyingType);
        }

        #endregion
    }

    /// <summary>
    /// 步骤执行上下文
    /// </summary>
    public class StepExecutionContext
    {
        /// <summary>
        /// WorkflowCore 原始上下文
        /// </summary>
        public IStepExecutionContext WorkflowContext { get; set; } = null!;

        /// <summary>
        /// 工作流数据
        /// </summary>
        public WorkflowStepData WorkflowData { get; set; } = null!;

        /// <summary>
        /// 当前步骤ID
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// 当前步骤名称
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 取消令牌
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 获取变量
        /// </summary>
        public T? GetVariable<T>(string name) => WorkflowData.GetVariable<T>(name);

        /// <summary>
        /// 设置变量
        /// </summary>
        public void SetVariable(string name, object? value) => WorkflowData.SetVariable(name, value);

        /// <summary>
        /// 获取上一步输出
        /// </summary>
        public StepOutputData? GetPreviousOutput() => WorkflowData.LastStepOutput;

        /// <summary>
        /// 替换变量引用
        /// </summary>
        public string ReplaceVariables(string template) => WorkflowData.ReplaceVariables(template);

        /// <summary>
        /// 记录日志
        /// </summary>
        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            WorkflowData.AddLog(message, level, StepId);
        }
    }

    /// <summary>
    /// 步骤执行结果
    /// </summary>
    public class StepResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 输出数据
        /// </summary>
        public Dictionary<string, object?> OutputData { get; set; } = new();

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 是否继续执行下一步
        /// </summary>
        public bool ProceedToNext { get; set; } = true;

        /// <summary>
        /// 指定下一步名称（用于分支）
        /// </summary>
        public string? NextStepName { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static StepResult Succeed(Dictionary<string, object?>? output = null)
        {
            return new StepResult
            {
                Success = true,
                OutputData = output ?? new Dictionary<string, object?>()
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static StepResult Fail(string errorMessage)
        {
            return new StepResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// 创建分支结果
        /// </summary>
        public static StepResult Branch(string nextStepName, Dictionary<string, object?>? output = null)
        {
            return new StepResult
            {
                Success = true,
                OutputData = output ?? new Dictionary<string, object?>(),
                NextStepName = nextStepName
            };
        }

        /// <summary>
        /// 创建暂停结果（等待外部事件）
        /// </summary>
        public static StepResult Suspend(Dictionary<string, object?>? output = null)
        {
            return new StepResult
            {
                Success = true,
                OutputData = output ?? new Dictionary<string, object?>(),
                ProceedToNext = false
            };
        }
    }
}
