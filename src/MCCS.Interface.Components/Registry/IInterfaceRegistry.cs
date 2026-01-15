using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.ViewModels;

namespace MCCS.Interface.Components.Registry
{
    /// <summary>
    /// 界面组件注册表接口 - 管理所有可用的界面组件
    /// </summary>
    public interface IInterfaceRegistry
    {
        /// <summary>
        /// 注册界面组件类型（UserControl）
        /// </summary>
        /// <typeparam name="TViewModel">视图模型</typeparam>
        void RegisterComponent<TViewModel>() where TViewModel : BaseComponentViewModel; 

        /// <summary>
        /// 注册界面组件类型
        /// </summary>
        void RegisterComponent(Type viewType);

        /// <summary>
        /// 获取所有已注册的组件信息
        /// </summary>
        IReadOnlyList<InterfaceInfo> GetAllComponents();

        /// <summary>
        /// 按分类获取组件
        /// </summary>
        IReadOnlyList<InterfaceInfo> GetComponentsByCategory(InterfaceComponentCategory category);

        /// <summary>
        /// 搜索组件
        /// </summary>
        IReadOnlyList<InterfaceInfo> SearchComponents(string keyword);

        /// <summary>
        /// 根据ID获取组件信息
        /// </summary>
        InterfaceInfo? GetComponentInfo(string componentId);

        /// <summary>
        /// 获取组件类型
        /// </summary>
        Type? GetComponentType(string componentId);

        /// <summary>
        /// 创建组件实例
        /// </summary>
        BaseComponentViewModel? CreateComponent(string componentId);

        /// <summary>
        /// 创建组件实例（带参数）
        /// </summary>
        BaseComponentViewModel? CreateComponent(string componentId, object? parameter);

        /// <summary>
        /// 创建组件实例（泛型）
        /// </summary>
        TViewModel? CreateComponent<TViewModel>(string componentId) where TViewModel : BaseComponentViewModel;

        /// <summary>
        /// 创建组件实例（泛型，带参数）
        /// </summary>
        TViewModel? CreateComponent<TViewModel>(string componentId, object? parameter) where TViewModel : BaseComponentViewModel;

        /// <summary>
        /// 检查组件是否已注册
        /// </summary>
        bool IsRegistered(string componentId);

        /// <summary>
        /// 取消注册组件
        /// </summary>
        bool Unregister(string componentId);

        /// <summary>
        /// 获取所有已注册的组件类型
        /// </summary>
        List<Type?> GetAllComponentTypes();

        /// <summary>
        /// 组件注册事件
        /// </summary>
        event EventHandler<InterfaceInfo>? ComponentRegistered;

        /// <summary>
        /// 组件注销事件
        /// </summary>
        event EventHandler<string>? ComponentUnregistered;
    }
}
