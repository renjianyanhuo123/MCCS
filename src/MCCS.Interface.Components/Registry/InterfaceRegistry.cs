using System.Reflection;
using System.Windows;

using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

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
        public void RegisterComponent<TView>() where TView : FrameworkElement, new()
        {
            RegisterComponent(typeof(TView), _ => new TView());
        }

        public void RegisterComponent<TView>(Func<IServiceProvider, TView> factory) where TView : FrameworkElement
        {
            RegisterComponent(typeof(TView), _ => factory(null!));
        }

        public void RegisterComponent(Type viewType)
        {
            if (!typeof(FrameworkElement).IsAssignableFrom(viewType))
            {
                throw new ArgumentException($"类型 {viewType.Name} 必须继承自 FrameworkElement");
            }

            RegisterComponent(viewType, resolver =>
            {
                // 优先尝试使用解析器从容器获取
                if (resolver != null)
                {
                    try
                    {
                        var instance = resolver(viewType);
                        if (instance is FrameworkElement element)
                        {
                            return element;
                        }
                    }
                    catch
                    {
                        // 解析失败，回退到 Activator
                    }
                }
                return (FrameworkElement)Activator.CreateInstance(viewType)!;
            });
        }

        private void RegisterComponent(Type viewType, Func<Func<Type, object>?, FrameworkElement> factory)
        {
            var attribute = viewType.GetCustomAttribute<InterfaceComponentAttribute>();
            var componentId = attribute?.Id ?? viewType.Name;

            // 获取关联的ViewModel类型（如果有）
            Type? viewModelType = FindViewModelType(viewType);

            var info = new InterfaceInfo
            {
                Id = componentId,
                Name = attribute?.Name ?? viewType.Name,
                Description = attribute?.Description ?? string.Empty,
                Category = attribute?.Category ?? InterfaceComponentCategory.Display,
                Icon = attribute?.Icon ?? "Cog",
                Version = attribute?.Version ?? "1.0.0",
                Author = attribute?.Author ?? string.Empty,
                IsEnabled = attribute?.IsEnabled ?? true,
                Order = attribute?.Order ?? 0,
                ViewType = viewType,
                ViewModelType = viewModelType
            };

            lock (_lock)
            {
                _registrations[componentId] = new ComponentRegistration
                {
                    Info = info,
                    Factory = factory
                };
            }

            // 触发注册事件
            ComponentRegistered?.Invoke(this, info);
        }

        private static Type? FindViewModelType(Type viewType)
        {
            // 尝试查找对应的 ViewModel 类型
            // 约定: ViewName -> ViewNameViewModel
            var viewModelTypeName = viewType.FullName + "ViewModel";
            var viewModelType = viewType.Assembly.GetType(viewModelTypeName);

            if (viewModelType == null)
            {
                // 尝试在 ViewModels 命名空间中查找
                var namespaceParts = viewType.Namespace?.Split('.') ?? [];
                if (namespaceParts.Length > 0)
                {
                    var baseNamespace = string.Join(".", namespaceParts.Take(namespaceParts.Length - 1));
                    viewModelTypeName = $"{baseNamespace}.ViewModels.{viewType.Name}ViewModel";
                    viewModelType = viewType.Assembly.GetType(viewModelTypeName);
                }
            }

            return viewModelType;
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
        public FrameworkElement? CreateComponent(string componentId)
        {
            return CreateComponent(componentId, null);
        }

        public FrameworkElement? CreateComponent(string componentId, object? parameter)
        {
            lock (_lock)
            {
                if (!_registrations.TryGetValue(componentId, out var registration))
                {
                    return null;
                }

                var component = registration.Factory(_serviceResolver);

                // 如果有参数且组件有 ViewModel，尝试设置参数
                if (parameter != null && component.DataContext != null)
                {
                    TrySetParameter(component.DataContext, parameter);
                }
                else if (parameter != null && registration.Info.ViewModelType != null)
                {
                    // 尝试使用参数创建 ViewModel
                    try
                    {
                        object? viewModel = TryCreateViewModel(registration.Info.ViewModelType, parameter);
                        if (viewModel != null)
                        {
                            component.DataContext = viewModel;
                        }
                    }
                    catch
                    {
                        // 如果无法使用参数创建，则尝试创建无参数实例后设置参数
                        try
                        {
                            var viewModel = TryCreateViewModel(registration.Info.ViewModelType, null);
                            if (viewModel != null)
                            {
                                TrySetParameter(viewModel, parameter);
                                component.DataContext = viewModel;
                            }
                        }
                        catch
                        {
                            // 忽略创建失败
                        }
                    }
                }

                return component;
            }
        }

        private object? TryCreateViewModel(Type viewModelType, object? parameter)
        {
            // 优先尝试使用解析器从容器获取
            if (_serviceResolver != null)
            {
                try
                {
                    return _serviceResolver(viewModelType);
                }
                catch
                {
                    // 解析失败，继续尝试其他方式
                }
            }

            // 尝试使用带参数的构造函数
            if (parameter != null)
            {
                try
                {
                    return Activator.CreateInstance(viewModelType, parameter);
                }
                catch
                {
                    // 带参数创建失败，继续尝试
                }
            }

            // 尝试无参数构造函数
            return Activator.CreateInstance(viewModelType);
        }

        public TView? CreateComponent<TView>(string componentId) where TView : FrameworkElement
        {
            return CreateComponent(componentId) as TView;
        }

        public TView? CreateComponent<TView>(string componentId, object? parameter) where TView : FrameworkElement
        {
            return CreateComponent(componentId, parameter) as TView;
        }

        private static void TrySetParameter(object viewModel, object parameter)
        {
            // 尝试查找并调用 Initialize 或 SetParameter 方法
            var initMethod = viewModel.GetType().GetMethod("Initialize", [parameter.GetType()]);
            if (initMethod != null)
            {
                initMethod.Invoke(viewModel, [parameter]);
                return;
            }

            var setParamMethod = viewModel.GetType().GetMethod("SetParameter", [parameter.GetType()]);
            if (setParamMethod != null)
            {
                setParamMethod.Invoke(viewModel, [parameter]);
                return;
            }

            // 尝试设置 Parameter 属性
            var paramProperty = viewModel.GetType().GetProperty("Parameter");
            if (paramProperty != null && paramProperty.CanWrite && paramProperty.PropertyType.IsAssignableFrom(parameter.GetType()))
            {
                paramProperty.SetValue(viewModel, parameter);
            }
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
        /// 自动发现并注册程序集中的所有界面组件
        /// </summary>
        public void DiscoverAndRegister(Assembly assembly)
        {
            var componentTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                            typeof(FrameworkElement).IsAssignableFrom(t) &&
                            t.GetCustomAttribute<InterfaceComponentAttribute>() != null);

            foreach (var type in componentTypes)
            {
                RegisterComponent(type);
            }
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
            public Func<Func<Type, object>?, FrameworkElement> Factory { get; init; } = null!;
        }
        #endregion
    }
}
