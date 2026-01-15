using System.Collections.Concurrent;
using System.Reflection;

using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.ViewModels;

namespace MCCS.Interface.Components.Registry
{
    /// <summary>
    /// 界面组件注册表实现（不依赖外部容器）
    /// </summary>
    public sealed class InterfaceRegistry : IInterfaceRegistry
    {
        private static readonly ConcurrentDictionary<Type, CtorPlan> _ctorPlanCache = new();

        /// <summary>
        /// 
        /// </summary>
        private sealed class CtorPlan
        {
            public ConstructorInfo Ctor { get; init; } = null!;
            public Type? ParamType { get; init; } // null 表示无参构造
        }

        private static CtorPlan GetCtorPlan(Type viewModelType) => _ctorPlanCache.GetOrAdd(viewModelType, BuildCtorPlan);

        private static CtorPlan BuildCtorPlan(Type viewModelType)
        {
            var ctors = viewModelType.GetConstructors();

            // 规则建议：
            // 1) 如果只有一个单参构造，就选它（你说每个子类都有一个不同参数类型 -> 通常这里成立）
            // 2) 否则优先无参构造
            // 3) 如果有多个单参构造 -> 明确报错（避免 silent bug）
            var singleParamCtors = ctors.Where(c => c.GetParameters().Length == 1).ToList();
            if (singleParamCtors.Count == 1)
            {
                var p = singleParamCtors[0].GetParameters()[0].ParameterType;
                return new CtorPlan { Ctor = singleParamCtors[0], ParamType = p };
            }

            var defaultCtor = ctors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (defaultCtor != null)
            {
                return new CtorPlan { Ctor = defaultCtor, ParamType = null };
            }

            if (singleParamCtors.Count > 1)
            {
                var sigs = string.Join(", ",
                    singleParamCtors.Select(c => $"({c.GetParameters()[0].ParameterType.Name})"));
                throw new InvalidOperationException(
                    $"类型 {viewModelType.Name} 存在多个单参数构造函数，无法确定使用哪个: {sigs}");
            }

            // 没无参，也没单参
            throw new InvalidOperationException(
                $"类型 {viewModelType.Name} 必须提供无参构造或单参数构造函数");
        }

        private readonly Dictionary<string, ComponentRegistration> _registrations = new(); 
        private readonly object _lock = new();

        #region Events
        public event EventHandler<InterfaceInfo>? ComponentRegistered;
        public event EventHandler<string>? ComponentUnregistered;
        #endregion

        #region Register Methods

        /// <summary>
        /// 注册组件（默认：根据构造函数 + 可选 parameter 创建实例）
        /// </summary>
        public void RegisterComponent<TViewModel>() where TViewModel : BaseComponentViewModel
        {
            var viewModelType = typeof(TViewModel);
            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();

            if (attribute == null)
                throw new InvalidOperationException($"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");

            var info = CreateInterfaceInfo(attribute, viewModelType);

            RegisterInternal(info, parameter => CreateComponentCache.CreateInstanceWithParameter(viewModelType, parameter));
        }

        /// <summary>
        /// 注册组件（自定义工厂：接收 object? parameter）
        /// </summary>
        public void RegisterComponent<TViewModel>(Func<object?, TViewModel> factory)
            where TViewModel : BaseComponentViewModel
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var viewModelType = typeof(TViewModel);
            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();

            if (attribute == null)
                throw new InvalidOperationException($"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");

            var info = CreateInterfaceInfo(attribute, viewModelType);

            RegisterInternal(info, factory);
        }

        /// <summary>
        /// 注册界面组件类型（强类型参数工厂）
        /// </summary>
        public void RegisterComponent<TViewModel, TParameter>(Func<TParameter, TViewModel> factory)
            where TViewModel : BaseComponentViewModel
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var viewModelType = typeof(TViewModel);
            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();

            if (attribute == null)
                throw new InvalidOperationException($"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");

            var info = CreateInterfaceInfo(attribute, viewModelType);
            info.ParameterType = typeof(TParameter);

            RegisterInternal(info, parameter =>
            {
                if (parameter is TParameter typed)
                    return factory(typed);

                throw new ArgumentException(
                    $"创建组件 '{info.Id}' 需要类型为 {typeof(TParameter).Name} 的参数",
                    nameof(parameter));
            });
        }

        public void RegisterComponent(Type viewModelType)
        {
            if (viewModelType == null) throw new ArgumentNullException(nameof(viewModelType));

            if (!typeof(BaseComponentViewModel).IsAssignableFrom(viewModelType))
                throw new ArgumentException($"类型 {viewModelType.Name} 必须继承自 BaseComponentViewModel", nameof(viewModelType));

            var attribute = viewModelType.GetCustomAttribute<InterfaceComponentAttribute>();
            if (attribute == null)
                throw new InvalidOperationException($"类型 {viewModelType.Name} 未标记 InterfaceComponentAttribute 特性");

            var info = CreateInterfaceInfo(attribute, viewModelType);

            RegisterInternal(info, parameter => CreateComponentCache.CreateInstanceWithParameter(viewModelType, parameter));
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
                return GetAllComponents();

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
                return _registrations.TryGetValue(componentId, out var reg) ? reg.Info : null;
        }

        public Type? GetComponentType(string componentId)
        {
            lock (_lock)
                return _registrations.TryGetValue(componentId, out var reg) ? reg.Info.ViewModelType : null;
        }

        public List<Type?> GetAllComponentTypes()
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled)
                    .Select(r => r.Info.ViewModelType)
                    .Cast<Type?>()
                    .ToList();
            }
        }
        #endregion

        #region Create Methods
        public BaseComponentViewModel? CreateComponent(string componentId) => CreateComponent(componentId, null);

        public BaseComponentViewModel? CreateComponent(string componentId, object? parameter)
        {
            ComponentRegistration reg;
            lock (_lock)
            {
                if (!_registrations.TryGetValue(componentId, out reg!))
                    return null;
            }

            return reg.Factory(parameter);
        }

        public TViewModel? CreateComponent<TViewModel>(string componentId)
            where TViewModel : BaseComponentViewModel
            => CreateComponent(componentId) as TViewModel;

        public TViewModel? CreateComponent<TViewModel>(string componentId, object? parameter)
            where TViewModel : BaseComponentViewModel
            => CreateComponent(componentId, parameter) as TViewModel;
        #endregion

        #region Management Methods
        public bool IsRegistered(string componentId)
        {
            lock (_lock)
                return _registrations.ContainsKey(componentId);
        }

        public bool Unregister(string componentId)
        {
            var removed = false;
            lock (_lock)
            {
                removed = _registrations.Remove(componentId);
            }

            // 锁外触发事件，避免订阅者回调引发死锁
            if (removed)
                ComponentUnregistered?.Invoke(this, componentId);

            return removed;
        }
        #endregion

        #region Discovery Methods
        public void DiscoverAndRegister(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var viewModelTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                            typeof(BaseComponentViewModel).IsAssignableFrom(t) &&
                            t.GetCustomAttribute<InterfaceComponentAttribute>() != null);

            foreach (var vmType in viewModelTypes)
            {
                try
                {
                    RegisterComponent(vmType);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("已经被注册"))
                {
                    // 重复注册：忽略
                }
            }
        }

        public void DiscoverAndRegisterFromCurrentAssembly() => DiscoverAndRegister(Assembly.GetExecutingAssembly());
        public void DiscoverAndRegisterFromCallingAssembly() => DiscoverAndRegister(Assembly.GetCallingAssembly());
        #endregion

        #region Private Helper Methods

        private static InterfaceInfo CreateInterfaceInfo(InterfaceComponentAttribute attribute, Type viewModelType) =>
            new()
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Description = attribute.Description,
                Category = attribute.Category,
                Icon = attribute.Icon,
                Version = attribute.Version,
                IsCanSetParam = attribute.IsCanSetParam,
                SetParamViewName = attribute.SetParamViewName,
                Author = attribute.Author,
                IsEnabled = attribute.IsEnabled,
                Order = attribute.Order,
                ViewModelType = viewModelType,
                ParameterType = GetParameterType(viewModelType)
            };

        /// <summary>
        /// 获取组件的构造参数类型（优先：公共单参数构造）
        /// </summary>
        private static Type? GetParameterType(Type viewModelType)
        {
            var constructors = viewModelType.GetConstructors();

            var singleParamCtor = constructors.FirstOrDefault(c => c.GetParameters().Length == 1);
            return singleParamCtor?.GetParameters()[0].ParameterType;
        }
         
        private void RegisterInternal(InterfaceInfo info, Func<object?, BaseComponentViewModel> factory)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            lock (_lock)
            {
                if (_registrations.ContainsKey(info.Id))
                    throw new InvalidOperationException($"组件 ID '{info.Id}' 已经被注册");

                _registrations[info.Id] = new ComponentRegistration
                {
                    Info = info,
                    Factory = factory
                };
            }

            // 锁外触发事件
            ComponentRegistered?.Invoke(this, info);
        }
         

        #endregion

        #region Private Classes
        private class ComponentRegistration
        {
            public InterfaceInfo Info { get; init; } = null!;
            public Func<object?, BaseComponentViewModel> Factory { get; init; } = null!;
        }
        #endregion
    }
}
