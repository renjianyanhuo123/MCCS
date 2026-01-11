using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;

namespace MCCS.Workflow.StepComponents.Registry
{
    /// <summary>
    /// 步骤注册表接口 - 管理所有可用的工作流步骤
    /// </summary>
    public interface IStepRegistry
    {
        /// <summary>
        /// 注册步骤类型
        /// </summary>
        void RegisterStep<TStep>() where TStep : BaseWorkflowStep, new();

        /// <summary>
        /// 注册步骤类型（使用工厂方法）
        /// </summary>
        void RegisterStep<TStep>(Func<IServiceProvider, TStep> factory) where TStep : BaseWorkflowStep;

        /// <summary>
        /// 注册步骤类型
        /// </summary>
        void RegisterStep(Type stepType);

        /// <summary>
        /// 获取所有已注册的步骤信息
        /// </summary>
        IReadOnlyList<StepInfo> GetAllSteps();

        /// <summary>
        /// 按分类获取步骤
        /// </summary>
        IReadOnlyList<StepInfo> GetStepsByCategory(ComponentCategory category);

        /// <summary>
        /// 搜索步骤
        /// </summary>
        IReadOnlyList<StepInfo> SearchSteps(string keyword);

        /// <summary>
        /// 根据ID获取步骤信息
        /// </summary>
        StepInfo? GetStepInfo(string stepId);

        /// <summary>
        /// 获取步骤类型
        /// </summary>
        Type? GetStepType(string stepId);

        /// <summary>
        /// 创建步骤实例
        /// </summary>
        BaseWorkflowStep? CreateStep(string stepId);

        /// <summary>
        /// 创建步骤实例（泛型）
        /// </summary>
        TStep? CreateStep<TStep>(string stepId) where TStep : BaseWorkflowStep;

        /// <summary>
        /// 检查步骤是否已注册
        /// </summary>
        bool IsRegistered(string stepId);

        /// <summary>
        /// 取消注册步骤
        /// </summary>
        bool Unregister(string stepId);

        /// <summary>
        /// 获取所有已注册的步骤类型
        /// </summary>
        IReadOnlyList<Type> GetAllStepTypes();
    }
}
