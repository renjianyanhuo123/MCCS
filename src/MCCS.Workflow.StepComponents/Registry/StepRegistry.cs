using System.Reflection;
using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Workflow.StepComponents.Registry
{
    /// <summary>
    /// 步骤注册表实现
    /// </summary>
    public class StepRegistry : IStepRegistry
    {
        private readonly Dictionary<string, StepRegistration> _registrations = new();
        private readonly IServiceProvider? _serviceProvider;
        private readonly object _lock = new();

        public StepRegistry(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        public void RegisterStep<TStep>() where TStep : BaseWorkflowStep, new()
        {
            RegisterStep(typeof(TStep), _ => new TStep());
        }

        public void RegisterStep<TStep>(Func<IServiceProvider, TStep> factory) where TStep : BaseWorkflowStep => RegisterStep(typeof(TStep), factory);

        public void RegisterStep(Type stepType)
        {
            if (!typeof(BaseWorkflowStep).IsAssignableFrom(stepType))
            {
                throw new ArgumentException($"类型 {stepType.Name} 必须继承自 BaseWorkflowStep");
            }

            RegisterStep(stepType, sp =>
            {
                if (sp != null)
                {
                    return (BaseWorkflowStep)ActivatorUtilities.CreateInstance(sp, stepType);
                }
                return (BaseWorkflowStep)Activator.CreateInstance(stepType)!;
            });
        }

        private void RegisterStep(Type stepType, Func<IServiceProvider?, BaseWorkflowStep> factory)
        {
            var attribute = stepType.GetCustomAttribute<StepComponentAttribute>();
            var stepId = attribute?.Id ?? stepType.Name;

            // 创建临时实例获取参数定义
            IReadOnlyList<Parameters.IComponentParameter>? parameterDefs = null;
            try
            {
                var tempInstance = factory(null);
                parameterDefs = tempInstance.GetParameterDefinitions();
            }
            catch
            {
                // 如果创建失败，忽略参数定义
            }

            var info = new StepInfo
            {
                Id = stepId,
                Name = attribute?.Name ?? stepType.Name,
                Description = attribute?.Description ?? string.Empty,
                Category = attribute?.Category ?? ComponentCategory.General,
                Icon = attribute?.Icon ?? "Cog",
                Version = attribute?.Version ?? "1.0.0",
                Author = attribute?.Author ?? string.Empty,
                Tags = attribute?.Tags ?? Array.Empty<string>(),
                IsEnabled = attribute?.IsEnabled ?? true,
                Order = attribute?.Order ?? 0,
                StepType = stepType,
                ParameterDefinitions = parameterDefs
            };

            lock (_lock)
            {
                _registrations[stepId] = new StepRegistration
                {
                    Info = info,
                    Factory = factory
                };
            }
        }

        public IReadOnlyList<StepInfo> GetAllSteps()
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled)
                    .Select(r => r.Info)
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.Order)
                    .ThenBy(s => s.Name)
                    .ToList();
            }
        }

        public IReadOnlyList<StepInfo> GetStepsByCategory(ComponentCategory category)
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled && r.Info.Category == category)
                    .Select(r => r.Info)
                    .OrderBy(s => s.Order)
                    .ThenBy(s => s.Name)
                    .ToList();
            }
        }

        public IReadOnlyList<StepInfo> SearchSteps(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return GetAllSteps();
            }

            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled &&
                        (r.Info.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                         r.Info.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                         r.Info.Tags.Any(t => t.Contains(keyword, StringComparison.OrdinalIgnoreCase))))
                    .Select(r => r.Info)
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.Order)
                    .ThenBy(s => s.Name)
                    .ToList();
            }
        }

        public StepInfo? GetStepInfo(string stepId)
        {
            lock (_lock)
            {
                return _registrations.TryGetValue(stepId, out var registration)
                    ? registration.Info
                    : null;
            }
        }

        public Type? GetStepType(string stepId)
        {
            lock (_lock)
            {
                return _registrations.TryGetValue(stepId, out var registration)
                    ? registration.Info.StepType
                    : null;
            }
        }

        public BaseWorkflowStep? CreateStep(string stepId)
        {
            lock (_lock)
            {
                if (_registrations.TryGetValue(stepId, out var registration))
                {
                    return registration.Factory(_serviceProvider);
                }
                return null;
            }
        }

        public TStep? CreateStep<TStep>(string stepId) where TStep : BaseWorkflowStep
        {
            return CreateStep(stepId) as TStep;
        }

        public bool IsRegistered(string stepId)
        {
            lock (_lock)
            {
                return _registrations.ContainsKey(stepId);
            }
        }

        public bool Unregister(string stepId)
        {
            lock (_lock)
            {
                return _registrations.Remove(stepId);
            }
        }

        public IReadOnlyList<Type> GetAllStepTypes()
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Where(r => r.Info.IsEnabled)
                    .Select(r => r.Info.StepType)
                    .ToList();
            }
        }

        /// <summary>
        /// 自动发现并注册程序集中的所有步骤
        /// </summary>
        public void DiscoverAndRegister(Assembly assembly)
        {
            var stepTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                           !t.IsInterface &&
                           typeof(BaseWorkflowStep).IsAssignableFrom(t) &&
                           t.GetCustomAttribute<StepComponentAttribute>() != null);

            foreach (var type in stepTypes)
            {
                RegisterStep(type);
            }
        }

        /// <summary>
        /// 自动发现并注册当前程序集中的所有步骤
        /// </summary>
        public void DiscoverAndRegisterFromCurrentAssembly()
        {
            DiscoverAndRegister(Assembly.GetExecutingAssembly());
        }

        private class StepRegistration
        {
            public StepInfo Info { get; set; } = null!;
            public Func<IServiceProvider?, BaseWorkflowStep> Factory { get; set; } = null!;
        }
    }
}
