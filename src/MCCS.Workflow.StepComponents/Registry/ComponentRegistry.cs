using System.Reflection;
using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;

namespace MCCS.Workflow.StepComponents.Registry
{
    /// <summary>
    /// 组件注册表实现
    /// </summary>
    public class ComponentRegistry : IComponentRegistry
    {
        private readonly Dictionary<string, ComponentRegistration> _registrations = new();
        private readonly object _lock = new();

        public void Register<TComponent>() where TComponent : IStepComponent, new()
        {
            Register(typeof(TComponent), () => new TComponent());
        }

        public void Register<TComponent>(Func<TComponent> factory) where TComponent : IStepComponent
        {
            Register(typeof(TComponent), () => factory());
        }

        public void Register(Type componentType)
        {
            if (!typeof(IStepComponent).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"类型 {componentType.Name} 必须实现 IStepComponent 接口");
            }

            Register(componentType, () => (IStepComponent)Activator.CreateInstance(componentType)!);
        }

        private void Register(Type componentType, Func<IStepComponent> factory)
        {
            var attribute = componentType.GetCustomAttribute<StepComponentAttribute>();
            var componentId = attribute?.Id ?? componentType.Name;

            var info = new ComponentInfo
            {
                Id = componentId,
                Name = attribute?.Name ?? componentType.Name,
                Description = attribute?.Description ?? string.Empty,
                Category = attribute?.Category ?? ComponentCategory.General,
                Icon = attribute?.Icon ?? "Cog",
                Version = attribute?.Version ?? "1.0.0",
                Author = attribute?.Author ?? string.Empty,
                Tags = attribute?.Tags ?? Array.Empty<string>(),
                IsEnabled = attribute?.IsEnabled ?? true,
                Order = attribute?.Order ?? 0,
                ComponentType = componentType
            };

            lock (_lock)
            {
                _registrations[componentId] = new ComponentRegistration
                {
                    Info = info,
                    Factory = factory
                };
            }
        }

        public IReadOnlyList<ComponentInfo> GetAllComponents()
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

        public IReadOnlyList<ComponentInfo> GetComponentsByCategory(ComponentCategory category)
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

        public IReadOnlyList<ComponentInfo> SearchComponents(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return GetAllComponents();
            }

            var lowerKeyword = keyword.ToLowerInvariant();

            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled &&
                        (r.Info.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                         r.Info.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                         r.Info.Tags.Any(t => t.Contains(keyword, StringComparison.OrdinalIgnoreCase))))
                    .Select(r => r.Info)
                    .OrderBy(c => c.Category)
                    .ThenBy(c => c.Order)
                    .ThenBy(c => c.Name)
                    .ToList();
            }
        }

        public ComponentInfo? GetComponentInfo(string componentId)
        {
            lock (_lock)
            {
                return _registrations.TryGetValue(componentId, out var registration)
                    ? registration.Info
                    : null;
            }
        }

        public IStepComponent? CreateComponent(string componentId)
        {
            lock (_lock)
            {
                if (_registrations.TryGetValue(componentId, out var registration))
                {
                    return registration.Factory();
                }
                return null;
            }
        }

        public TComponent? CreateComponent<TComponent>(string componentId) where TComponent : class, IStepComponent
        {
            return CreateComponent(componentId) as TComponent;
        }

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
                return _registrations.Remove(componentId);
            }
        }

        /// <summary>
        /// 自动发现并注册程序集中的所有组件
        /// </summary>
        public void DiscoverAndRegister(Assembly assembly)
        {
            var componentTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                           !t.IsInterface &&
                           typeof(IStepComponent).IsAssignableFrom(t) &&
                           t.GetCustomAttribute<StepComponentAttribute>() != null);

            foreach (var type in componentTypes)
            {
                Register(type);
            }
        }

        /// <summary>
        /// 自动发现并注册当前程序集中的所有组件
        /// </summary>
        public void DiscoverAndRegisterFromCurrentAssembly()
        {
            DiscoverAndRegister(Assembly.GetExecutingAssembly());
        }

        private class ComponentRegistration
        {
            public ComponentInfo Info { get; set; } = null!;
            public Func<IStepComponent> Factory { get; set; } = null!;
        }
    }
}
