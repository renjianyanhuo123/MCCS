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

            RegisterInternal(info, Factory);
            return;

            BaseComponentViewModel Factory(Func<Type, object>? resolver, object? parameter)
            {
                return CreateInstanceWithParameter(viewModelType, resolver, parameter);
            }
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

            RegisterInternal(info, InternalFactory);
            return;

            BaseComponentViewModel InternalFactory(Func<Type, object>? resolver, object? parameter)
            {
                if (resolver == null) return CreateInstanceWithParameter(viewModelType, null, parameter);
                var serviceProvider = new ServiceProviderWrapper(resolver);
                return factory(serviceProvider); 
            }
        }

        /// <summary>
        /// 注册界面组件类型（使用带参数的工厂方法）
        /// </summary>
        /// <typeparam name="TViewModel">视图模型类型</typeparam>
        /// <typeparam name="TParameter">构造参数类型</typeparam>
        /// <param name="factory">工厂方法，接收参数并返回 ViewModel 实例</param>
        public void RegisterComponent<TViewModel, TParameter>(Func<TParameter, TViewModel> factory)
            where TViewModel : BaseComponentViewModel
        {
            var viewModelType = typeof(TViewModel);
            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");
            }

            var info = CreateInterfaceInfo(attribute, viewModelType);
            // 明确设置参数类型
            info.ParameterType = typeof(TParameter);

            RegisterInternal(info, InternalFactory);
            return;

            BaseComponentViewModel InternalFactory(Func<Type, object>? resolver, object? parameter)
            {
                if (parameter is TParameter typedParameter)
                {
                    return factory(typedParameter);
                }

                throw new ArgumentException(
                    $"创建组件 '{info.Id}' 需要类型为 {typeof(TParameter).Name} 的参数",
                    nameof(parameter));
            }
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

            BaseComponentViewModel Factory(Func<Type, object>? resolver, object? parameter)
            {
                return CreateInstanceWithParameter(viewModelType, resolver, parameter);
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
                    ? registration.Info.ViewModelType
                    : null;
            }
        }

        public List<Type?> GetAllComponentTypes()
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled)
                    .Select(r => r.Info.ViewModelType)
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

            // 通过工厂方法创建实例，参数直接传递给构造函数
            var viewModel = registration.Factory(_serviceResolver, parameter);

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
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
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
        private static InterfaceInfo CreateInterfaceInfo(InterfaceComponentAttribute attribute, Type viewModelType) =>
            new()
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
                ViewModelType = viewModelType,
                ParameterType = GetParameterType(viewModelType)
            };

        /// <summary>
        /// 获取组件的构造参数类型（从构造函数中获取）
        /// </summary>
        /// <remarks>
        /// 查找顺序：
        /// 1. 优先查找带单个参数的公共构造函数
        /// 2. 如果没有，返回 null（表示无参构造）
        /// </remarks>
        private static Type? GetParameterType(Type viewModelType)
        {
            // 获取所有公共构造函数
            var constructors = viewModelType.GetConstructors();

            // 优先查找带单个参数的构造函数
            var singleParamCtor = constructors
                .FirstOrDefault(c => c.GetParameters().Length == 1);

            if (singleParamCtor != null)
            {
                return singleParamCtor.GetParameters()[0].ParameterType;
            }

            // 没有带参数的构造函数
            return null;
        }

        /// <summary>
        /// 通过构造参数创建 ViewModel 实例
        /// </summary>
        /// <param name="viewModelType">ViewModel 类型</param>
        /// <param name="resolver">服务解析器</param>
        /// <param name="parameter">构造参数（可为 null）</param>
        /// <returns>创建的 ViewModel 实例</returns>
        private static BaseComponentViewModel CreateInstanceWithParameter(
            Type viewModelType,
            Func<Type, object>? resolver,
            object? parameter)
        {
            var constructors = viewModelType.GetConstructors();

            // 如果有参数，优先查找匹配参数类型的构造函数
            if (parameter != null)
            {
                var paramType = parameter.GetType();

                // 查找参数类型完全匹配的构造函数
                var matchingCtor = constructors
                    .FirstOrDefault(c =>
                    {
                        var ctorParams = c.GetParameters();
                        return ctorParams.Length == 1 && ctorParams[0].ParameterType.IsAssignableFrom(paramType);
                    });

                if (matchingCtor != null)
                {
                    return (BaseComponentViewModel)matchingCtor.Invoke([parameter]);
                }

                // 如果没有找到匹配的构造函数，抛出异常
                throw new InvalidOperationException(
                    $"类型 {viewModelType.Name} 没有接受 {paramType.Name} 类型参数的构造函数");
            }

            // 如果没有参数，查找无参构造函数
            var defaultCtor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (defaultCtor != null)
            {
                return (BaseComponentViewModel)defaultCtor.Invoke(null);
            }

            // 如果使用服务解析器，尝试解析依赖
            if (resolver != null)
            {
                return (BaseComponentViewModel)resolver(viewModelType);
            }

            // 最后尝试使用 Activator.CreateInstance（可能会失败如果没有无参构造函数）
            throw new InvalidOperationException(
                $"类型 {viewModelType.Name} 需要构造参数才能创建实例，请在调用 CreateComponent 时提供参数");
        }

        /// <summary>
        /// 内部注册方法
        /// </summary>
        private void RegisterInternal(InterfaceInfo info, Func<Func<Type, object>?, object?, BaseComponentViewModel> factory)
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
            /// <summary>
            /// 工厂方法：(服务解析器, 构造参数) => ViewModel实例
            /// </summary>
            public Func<Func<Type, object>?, object?, BaseComponentViewModel> Factory { get; init; } = null!;
        }

        /// <summary>
        /// IServiceProvider 包装器，将 Func<Type, object> 适配为 IServiceProvider
        /// </summary>
        private class ServiceProviderWrapper(Func<Type, object> resolver) : IServiceProvider
        {
            private readonly Func<Type, object> _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

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
