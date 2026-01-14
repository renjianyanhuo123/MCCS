using System.Reflection;

using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.ViewModels;

namespace MCCS.Interface.Components.Registry
{
    /// <summary>
    /// 界面组件注册表实现
    /// </summary>
    public sealed class InterfaceRegistry : IInterfaceRegistry
    {
        private readonly Dictionary<string, ComponentRegistration> _registrations = new();
        private readonly Func<Type, object>? _serviceResolver;
        private readonly object _lock = new();

        public InterfaceRegistry()
        {
        }

        /// <summary>
        /// 使用服务解析器创建注册表
        /// </summary>
        /// <param name="serviceResolver">服务解析器，用于从容器中获取服务</param>
        public InterfaceRegistry(Func<Type, object>? serviceResolver)
        {
            _serviceResolver = serviceResolver;
        }

        #region Events
        public event EventHandler<InterfaceInfo>? ComponentRegistered;
        public event EventHandler<string>? ComponentUnregistered;
        #endregion

        #region Register Methods
        public void RegisterComponent<TViewModel>() where TViewModel : BaseComponentViewModel
        {
        }

        public void RegisterComponent<TViewModel>(Func<IServiceProvider, TViewModel> factory) where TViewModel : BaseComponentViewModel
        {
        }

        public void RegisterComponent(Type viewType)
        {

        }

        #endregion

        #region Query Methods
        public IReadOnlyList<InterfaceInfo> GetAllComponents()
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled)
                    .Select(r => r.Info)
                    .OrderBy(c => c.Category)
                    .ThenBy(c => c.Order)
                    .ThenBy(c => c.Name)
                    .ToList();
            }
        }

        public IReadOnlyList<InterfaceInfo> GetComponentsByCategory(InterfaceComponentCategory category)
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled && r.Info.Category == category)
                    .Select(r => r.Info)
                    .OrderBy(c => c.Order)
                    .ThenBy(c => c.Name)
                    .ToList();
            }
        }

        public IReadOnlyList<InterfaceInfo> SearchComponents(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return GetAllComponents();
            }

            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled &&
                        (r.Info.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                         r.Info.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                         r.Info.Id.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .Select(r => r.Info)
                    .OrderBy(c => c.Category)
                    .ThenBy(c => c.Order)
                    .ThenBy(c => c.Name)
                    .ToList();
            }
        }

        public InterfaceInfo? GetComponentInfo(string componentId)
        {
            lock (_lock)
            {
                return _registrations.TryGetValue(componentId, out var registration)
                    ? registration.Info
                    : null;
            }
        }

        public Type? GetComponentType(string componentId)
        {
            lock (_lock)
            {
                return _registrations.TryGetValue(componentId, out var registration)
                    ? registration.Info.ViewType
                    : null;
            }
        }

        public IReadOnlyList<Type> GetAllComponentTypes()
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled)
                    .Select(r => r.Info.ViewType)
                    .ToList();
            }
        }
        #endregion

        #region Create Methods


        #endregion

        #region Management Methods
        public bool IsRegistered(string componentId)
        {
            lock (_lock)
            {
                return _registrations.ContainsKey(componentId);
            }
        }

        public bool Unregister(string componentId)
        {
            lock (_lock)
            {
                if (_registrations.Remove(componentId))
                {
                    ComponentUnregistered?.Invoke(this, componentId);
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Discovery Methods
        /// <summary>
        /// 自动发现并注册程序集中的所有界面组件
        /// </summary>
        public void DiscoverAndRegister(Assembly assembly)
        {
        }

        /// <summary>
        /// 自动发现并注册当前程序集中的所有界面组件
        /// </summary>
        public void DiscoverAndRegisterFromCurrentAssembly()
        {
            DiscoverAndRegister(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// 自动发现并注册调用程序集中的所有界面组件
        /// </summary>
        public void DiscoverAndRegisterFromCallingAssembly()
        {
            DiscoverAndRegister(Assembly.GetCallingAssembly());
        }
        #endregion

        #region Private Classes
        private class ComponentRegistration
        {
            public InterfaceInfo Info { get; init; } = null!;
            public Func<Func<Type, object>?, BaseComponentViewModel> Factory { get; init; } = null!;
        }
        #endregion
    }
}
