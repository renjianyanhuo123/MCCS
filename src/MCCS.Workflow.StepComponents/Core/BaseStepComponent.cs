using System.Diagnostics;
using System.Reflection;
using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 步骤组件基类
    /// </summary>
    public abstract class BaseStepComponent : IStepComponent
    {
        private readonly StepComponentAttribute? _componentAttribute;
        private readonly List<IComponentParameter> _parameters;

        protected BaseStepComponent()
        {
            _componentAttribute = GetType().GetCustomAttribute<StepComponentAttribute>();
            _parameters = InitializeParameters();
        }

        #region IStepComponent 实现

        public virtual string Id => _componentAttribute?.Id ?? GetType().Name;
        public virtual string Name => _componentAttribute?.Name ?? GetType().Name;
        public virtual string Description => _componentAttribute?.Description ?? string.Empty;
        public virtual ComponentCategory Category => _componentAttribute?.Category ?? ComponentCategory.General;
        public virtual string Icon => _componentAttribute?.Icon ?? "Cog";
        public virtual string Version => _componentAttribute?.Version ?? "1.0.0";

        public IReadOnlyList<IComponentParameter> GetParameterDefinitions() => _parameters.AsReadOnly();

        public IDictionary<string, object?> GetParameterValues()
        {
            var values = new Dictionary<string, object?>();
            foreach (var param in _parameters)
            {
                values[param.Name] = param.Value;
            }
            return values;
        }

        public void SetParameterValues(IDictionary<string, object?> values)
        {
            foreach (var kvp in values)
            {
                var param = _parameters.FirstOrDefault(p => p.Name == kvp.Key);
                if (param != null)
                {
                    param.Value = kvp.Value;
                }
            }

            // 同步到属性
            SyncParametersToProperties();
        }

        public ComponentValidationResult Validate()
        {
            var result = new ComponentValidationResult { IsValid = true };

            foreach (var param in _parameters)
            {
                var paramResult = param.Validate();
                if (!paramResult.IsValid)
                {
                    result.AddError(param.Name, paramResult.ErrorMessage ?? "验证失败");
                }
            }

            // 调用自定义验证
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

        public async Task<ComponentExecutionResult> ExecuteAsync(ComponentExecutionContext context, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 同步参数到属性
                SyncParametersToProperties();

                // 执行前准备
                await OnBeforeExecuteAsync(context, cancellationToken);

                // 执行核心逻辑
                var result = await ExecuteCoreAsync(context, cancellationToken);

                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                // 执行后处理
                await OnAfterExecuteAsync(context, result, cancellationToken);

                return result;
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                return new ComponentExecutionResult
                {
                    Status = ComponentExecutionStatus.Cancelled,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                context.Log?.Invoke($"组件执行异常: {ex.Message}", LogLevel.Error);

                return new ComponentExecutionResult
                {
                    Status = ComponentExecutionStatus.Failed,
                    ErrorMessage = ex.Message,
                    Exception = ex,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public abstract IStepComponent Clone();

        #endregion

        #region 可重写的方法

        /// <summary>
        /// 执行核心逻辑（子类必须实现）
        /// </summary>
        protected abstract Task<ComponentExecutionResult> ExecuteCoreAsync(
            ComponentExecutionContext context,
            CancellationToken cancellationToken);

        /// <summary>
        /// 自定义验证（子类可重写）
        /// </summary>
        protected virtual ComponentValidationResult ValidateCustom()
        {
            return ComponentValidationResult.Valid();
        }

        /// <summary>
        /// 执行前回调（子类可重写）
        /// </summary>
        protected virtual Task OnBeforeExecuteAsync(ComponentExecutionContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 执行后回调（子类可重写）
        /// </summary>
        protected virtual Task OnAfterExecuteAsync(ComponentExecutionContext context, ComponentExecutionResult result, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 定义组件参数（子类必须实现）
        /// </summary>
        protected abstract IEnumerable<IComponentParameter> DefineParameters();

        #endregion

        #region 私有方法

        private List<IComponentParameter> InitializeParameters()
        {
            var parameters = DefineParameters().ToList();

            // 设置默认值
            foreach (var param in parameters)
            {
                if (param.Value == null && param.DefaultValue != null)
                {
                    param.Value = param.DefaultValue;
                }
            }

            return parameters;
        }

        private void SyncParametersToProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var param in _parameters)
            {
                var prop = properties.FirstOrDefault(p =>
                    p.GetCustomAttribute<ParameterAttribute>()?.Name == param.Name ||
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

        #region 辅助方法

        /// <summary>
        /// 获取参数值
        /// </summary>
        protected T? GetParameterValue<T>(string parameterName)
        {
            var param = _parameters.FirstOrDefault(p => p.Name == parameterName);
            if (param?.Value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        protected void SetParameterValue(string parameterName, object? value)
        {
            var param = _parameters.FirstOrDefault(p => p.Name == parameterName);
            if (param != null)
            {
                param.Value = value;
            }
        }

        /// <summary>
        /// 克隆参数列表
        /// </summary>
        protected List<IComponentParameter> CloneParameters()
        {
            return _parameters.Select(p => p.Clone()).ToList();
        }

        #endregion
    }
}
