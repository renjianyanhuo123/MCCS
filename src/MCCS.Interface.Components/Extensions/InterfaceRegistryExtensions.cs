using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Registry;

namespace MCCS.Interface.Components.Extensions
{
    /// <summary>
    /// IInterfaceRegistry 扩展方法
    /// </summary>
    public static class InterfaceRegistryExtensions
    {
        /// <summary>
        /// 获取所有展示类组件
        /// </summary>
        public static IReadOnlyList<InterfaceInfo> GetDisplayComponents(this IInterfaceRegistry registry)
        {
            return registry.GetComponentsByCategory(InterfaceComponentCategory.Display);
        }

        /// <summary>
        /// 获取所有交互类组件
        /// </summary>
        public static IReadOnlyList<InterfaceInfo> GetInteractionComponents(this IInterfaceRegistry registry)
        {
            return registry.GetComponentsByCategory(InterfaceComponentCategory.Interaction);
        }

        /// <summary>
        /// 创建组件并添加到容器中
        /// </summary>
        /// <param name="registry">注册表</param>
        /// <param name="componentId">组件ID</param>
        /// <param name="container">目标容器</param>
        /// <param name="parameter">可选参数</param>
        /// <returns>创建的组件，如果失败则返回null</returns>
        public static FrameworkElement? CreateAndAddToContainer(
            this IInterfaceRegistry registry,
            string componentId,
            Panel container,
            object? parameter = null)
        {
            var component = registry.CreateComponent(componentId, parameter);
            if (component != null)
            {
                container.Children.Add(component);
            }
            return component;
        }

        /// <summary>
        /// 创建组件并设置为ContentControl的内容
        /// </summary>
        /// <param name="registry">注册表</param>
        /// <param name="componentId">组件ID</param>
        /// <param name="contentControl">目标ContentControl</param>
        /// <param name="parameter">可选参数</param>
        /// <returns>创建的组件，如果失败则返回null</returns>
        public static FrameworkElement? CreateAndSetContent(
            this IInterfaceRegistry registry,
            string componentId,
            ContentControl contentControl,
            object? parameter = null)
        {
            var component = registry.CreateComponent(componentId, parameter);
            if (component != null)
            {
                contentControl.Content = component;
            }
            return component;
        }

        /// <summary>
        /// 批量创建组件
        /// </summary>
        /// <param name="registry">注册表</param>
        /// <param name="componentIds">组件ID列表</param>
        /// <returns>创建的组件字典</returns>
        public static Dictionary<string, FrameworkElement> CreateComponents(
            this IInterfaceRegistry registry,
            IEnumerable<string> componentIds)
        {
            var result = new Dictionary<string, FrameworkElement>();
            foreach (var id in componentIds)
            {
                var component = registry.CreateComponent(id);
                if (component != null)
                {
                    result[id] = component;
                }
            }
            return result;
        }

        /// <summary>
        /// 检查组件是否属于指定分类
        /// </summary>
        public static bool IsComponentOfCategory(
            this IInterfaceRegistry registry,
            string componentId,
            InterfaceComponentCategory category)
        {
            var info = registry.GetComponentInfo(componentId);
            return info?.Category == category;
        }

        /// <summary>
        /// 从外部程序集发现并注册组件
        /// </summary>
        public static void DiscoverAndRegisterFromAssembly(
            this IInterfaceRegistry registry,
            Assembly assembly)
        {
            if (registry is InterfaceRegistry interfaceRegistry)
            {
                interfaceRegistry.DiscoverAndRegister(assembly);
            }
        }

        /// <summary>
        /// 获取组件数量统计
        /// </summary>
        public static (int total, int display, int interaction) GetComponentCounts(
            this IInterfaceRegistry registry)
        {
            var all = registry.GetAllComponents();
            var displayCount = all.Count(c => c.Category == InterfaceComponentCategory.Display);
            var interactionCount = all.Count(c => c.Category == InterfaceComponentCategory.Interaction);
            return (all.Count, displayCount, interactionCount);
        }
    }
}
