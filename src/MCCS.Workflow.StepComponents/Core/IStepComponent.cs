namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 工作流步骤组件接口
    /// </summary>
    public interface IStepComponent
    {
        /// <summary>
        /// 组件唯一标识
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 组件名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 组件描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 组件分类
        /// </summary>
        ComponentCategory Category { get; }

        /// <summary>
        /// 组件图标（Material Design Icon名称）
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// 组件版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 获取组件参数定义
        /// </summary>
        IReadOnlyList<IComponentParameter> GetParameterDefinitions();

        /// <summary>
        /// 获取当前参数值
        /// </summary>
        IDictionary<string, object?> GetParameterValues();

        /// <summary>
        /// 设置参数值
        /// </summary>
        void SetParameterValues(IDictionary<string, object?> values);

        /// <summary>
        /// 验证参数
        /// </summary>
        ComponentValidationResult Validate();

        /// <summary>
        /// 执行组件
        /// </summary>
        Task<ComponentExecutionResult> ExecuteAsync(ComponentExecutionContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// 克隆组件（创建新实例）
        /// </summary>
        IStepComponent Clone();
    }
}
