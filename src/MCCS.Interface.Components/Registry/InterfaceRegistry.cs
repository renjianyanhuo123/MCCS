using System.Reflection;

using MCCS.Interface.Components.Attributes;
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
            var viewModelType = typeof(TViewModel);
            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");
            }

            var info = CreateInterfaceInfo(attribute, viewModelType);

            BaseComponentViewModel Factory(Func<Type, object>? resolver)
            {
                if (resolver != null)
                {
                    return (BaseComponentViewModel)resolver(viewModelType);
                }

                return Activator.CreateInstance<TViewModel>();
            }

            RegisterInternal(info, Factory);
        }

        public void RegisterComponent<TViewModel>(Func<IServiceProvider, TViewModel> factory) where TViewModel : BaseComponentViewModel
        {
            var viewModelType = typeof(TViewModel);
            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");
            }

            var info = CreateInterfaceInfo(attribute, viewModelType);

            BaseComponentViewModel InternalFactory(Func<Type, object>? resolver)
            {
                if (resolver != null)
                {
                    var serviceProvider = new ServiceProviderWrapper(resolver);
                    return factory(serviceProvider);
                }

                return Activator.CreateInstance<TViewModel>();
            }

            RegisterInternal(info, InternalFactory);
        }

        public void RegisterComponent(Type viewModelType)
        {
            if (viewModelType == null)
            {
                throw new ArgumentNullException(nameof(viewModelType));
            }

            if (!typeof(BaseComponentViewModel).IsAssignableFrom(viewModelType))
            {
                throw new ArgumentException(
                    $"类型 {viewModelType.Name} 必须继承自 BaseComponentViewModel",
                    nameof(viewModelType));
            }

            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");
            }

            var info = CreateInterfaceInfo(attribute, viewModelType);

            BaseComponentViewModel Factory(Func<Type, object>? resolver)
            {
                if (resolver != null)
                {
                    return (BaseComponentViewModel)resolver(viewModelType);
                }

                return (BaseComponentViewModel)Activator.CreateInstance(viewModelType)!;
            }

            RegisterInternal(info, Factory);
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
        public BaseComponentViewModel? CreateComponent(string componentId)
        {
            return CreateComponent(componentId, null);
        }

        public BaseComponentViewModel? CreateComponent(string componentId, object? parameter)
        {
            ComponentRegistration? registration;
            lock (_lock)
            {
                if (!_registrations.TryGetValue(componentId, out registration))
                {
                    return null;
                }
            }

            var viewModel = registration.Factory(_serviceResolver);

            // 如果 ViewModel 实现了 IInterfaceComponent 接口，进行初始化
            if (viewModel is Core.IInterfaceComponent component)
            {
                if (parameter != null)
                {
                    component.Initialize(parameter);
                }
                else
                {
                    component.Initialize();
                }
            }

            return viewModel;
        }

        public TViewModel? CreateComponent<TViewModel>(string componentId) where TViewModel : BaseComponentViewModel
        {
            return CreateComponent(componentId) as TViewModel;
        }

        public TViewModel? CreateComponent<TViewModel>(string componentId, object? parameter) where TViewModel : BaseComponentViewModel
        {
            return CreateComponent(componentId, parameter) as TViewModel;
        }
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
        /// 自动发现并注册程序集中的所有界面组件（ViewModel）
        /// </summary>
        public void DiscoverAndRegister(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            // 查找所有标记了 InterfaceComponentAttribute 的 ViewModel 类型
            var viewModelTypes = assembly.GetTypes()
                .Where(t => t.IsClass &&
                           !t.IsAbstract &&
                           typeof(BaseComponentViewModel).IsAssignableFrom(t) &&
                           t.GetCustomAttribute<InterfaceComponentAttribute>() != null);

            foreach (var viewModelType in viewModelTypes)
            {
                try
                {
                    RegisterComponent(viewModelType);
                }
                catch (Exception)
                {
                    // 忽略注册失败的组件（可能已经注册过）
                }
            }
        }

        /// <summary>
        /// 自动发现并注册当前程序集中的所有界面组件
        /// </summary>
        public void DiscoverAndRegisterFromCurrentAssembly() => DiscoverAndRegister(Assembly.GetExecutingAssembly());

        /// <summary>
        /// 自动发现并注册调用程序集中的所有界面组件
        /// </summary>
        public void DiscoverAndRegisterFromCallingAssembly() => DiscoverAndRegister(Assembly.GetCallingAssembly());

        #endregion

        #region Private Helper Methods
        /// <summary>
        /// 从特性创建 InterfaceInfo
        /// </summary>
        private static InterfaceInfo CreateInterfaceInfo(InterfaceComponentAttribute attribute, Type viewModelType)
        {
            return new InterfaceInfo
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Description = attribute.Description,
                Category = attribute.Category,
                Icon = attribute.Icon,
                Version = attribute.Version,
                Author = attribute.Author,
                IsEnabled = attribute.IsEnabled,
                Order = attribute.Order,
                ViewType = viewModelType, // ViewModel 类型作为 ViewType
                ViewModelType = viewModelType,
                ParameterType = GetParameterType(viewModelType)
            };
        }

        /// <summary>
        /// 获取组件的参数类型（如果实现了 IInterfaceComponent<TParameter>）
        /// </summary>
        private static Type? GetParameterType(Type viewModelType)
        {
            var interfaceType = viewModelType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                     i.GetGenericTypeDefinition() == typeof(Core.IInterfaceComponent<>));

            return interfaceType?.GetGenericArguments().FirstOrDefault();
        }

        /// <summary>
        /// 内部注册方法
        /// </summary>
        private void RegisterInternal(InterfaceInfo info, Func<Func<Type, object>?, BaseComponentViewModel> factory)
        {
            lock (_lock)
            {
                if (_registrations.ContainsKey(info.Id))
                {
                    throw new InvalidOperationException($"组件 ID '{info.Id}' 已经被注册");
                }

                var registration = new ComponentRegistration
                {
                    Info = info,
                    Factory = factory
                };

                _registrations[info.Id] = registration;
            }

            // 在锁外触发事件，避免死锁
            ComponentRegistered?.Invoke(this, info);
        }
        #endregion

        #region Private Classes
        /// <summary>
        /// 组件注册信息
        /// </summary>
        private class ComponentRegistration
        {
            public InterfaceInfo Info { get; init; } = null!;
            public Func<Func<Type, object>?, BaseComponentViewModel> Factory { get; init; } = null!;
        }

        /// <summary>
        /// IServiceProvider 包装器，将 Func<Type, object> 适配为 IServiceProvider
        /// </summary>
        private class ServiceProviderWrapper : IServiceProvider
        {
            private readonly Func<Type, object> _resolver;

            public ServiceProviderWrapper(Func<Type, object> resolver)
            {
                _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            }

            public object? GetService(Type serviceType)
            {
                try
                {
                    return _resolver(serviceType);
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion
    }
}
