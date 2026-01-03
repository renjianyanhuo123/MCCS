using MCCS.Workflow.StepComponents.Core;

namespace MCCS.Workflow.StepComponents.Registry
{
    /// <summary>
    /// 组件注册表接口
    /// </summary>
    public interface IComponentRegistry
    {
        /// <summary>
        /// 注册组件
        /// </summary>
        void Register<TComponent>() where TComponent : IStepComponent, new();

        /// <summary>
        /// 注册组件（使用工厂方法）
        /// </summary>
        void Register<TComponent>(Func<TComponent> factory) where TComponent : IStepComponent;

        /// <summary>
        /// 注册组件类型
        /// </summary>
        void Register(Type componentType);

        /// <summary>
        /// 获取所有注册的组件信息
        /// </summary>
        IReadOnlyList<ComponentInfo> GetAllComponents();

        /// <summary>
        /// 按分类获取组件
        /// </summary>
        IReadOnlyList<ComponentInfo> GetComponentsByCategory(ComponentCategory category);

        /// <summary>
        /// 按标签搜索组件
        /// </summary>
        IReadOnlyList<ComponentInfo> SearchComponents(string keyword);

        /// <summary>
        /// 根据ID获取组件信息
        /// </summary>
        ComponentInfo? GetComponentInfo(string componentId);

        /// <summary>
        /// 创建组件实例
        /// </summary>
        IStepComponent? CreateComponent(string componentId);

        /// <summary>
        /// 创建组件实例（泛型）
        /// </summary>
        TComponent? CreateComponent<TComponent>(string componentId) where TComponent : class, IStepComponent;

        /// <summary>
        /// 检查组件是否已注册
        /// </summary>
        bool IsRegistered(string componentId);

        /// <summary>
        /// 取消注册组件
        /// </summary>
        bool Unregister(string componentId);
    }
}
