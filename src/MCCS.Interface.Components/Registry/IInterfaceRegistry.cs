using System.Windows;

using MCCS.Interface.Components.Enums;

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
        /// <typeparam name="TView">视图类型</typeparam>
        void RegisterComponent<TView>() where TView : FrameworkElement, new();

        /// <summary>
        /// 注册界面组件类型（使用工厂方法）
        /// </summary>
        /// <typeparam name="TView">视图类型</typeparam>
        void RegisterComponent<TView>(Func<IServiceProvider, TView> factory) where TView : FrameworkElement;

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
        FrameworkElement? CreateComponent(string componentId);

        /// <summary>
        /// 创建组件实例（带参数）
        /// </summary>
        FrameworkElement? CreateComponent(string componentId, object? parameter);

        /// <summary>
        /// 创建组件实例（泛型）
        /// </summary>
        TView? CreateComponent<TView>(string componentId) where TView : FrameworkElement;

        /// <summary>
        /// 创建组件实例（泛型，带参数）
        /// </summary>
        TView? CreateComponent<TView>(string componentId, object? parameter) where TView : FrameworkElement;

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
        IReadOnlyList<Type> GetAllComponentTypes();

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
