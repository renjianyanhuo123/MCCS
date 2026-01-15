using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using MCCS.Interface.Components.ViewModels;

using Prism.Ioc;

namespace MCCS.Interface.Components.Registry
{
    /// <summary>
    /// 高性能组件激活器 - 使用编译表达式工厂创建 ViewModel 实例
    /// 支持混合参数注入：DI 服务依赖 + 业务参数
    /// </summary>
    public sealed class ComponentActivator
    {
        private readonly IContainerProvider _containerProvider;
        private readonly ConcurrentDictionary<Type, ActivatorPlan> _planCache = new();

        public ComponentActivator(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));
        }

        /// <summary>
        /// 创建 ViewModel 实例
        /// </summary>
        /// <param name="viewModelType">ViewModel 类型</param>
        /// <param name="businessParameter">业务参数（可选）</param>
        /// <returns>创建的 ViewModel 实例</returns>
        public BaseComponentViewModel CreateInstance(Type viewModelType, object? businessParameter)
        {
            var plan = _planCache.GetOrAdd(viewModelType, BuildActivatorPlan);
            return plan.Activate(_containerProvider, businessParameter);
        }

        /// <summary>
        /// 获取 ViewModel 的业务参数类型（用于参数转换）
        /// </summary>
        public Type? GetBusinessParameterType(Type viewModelType)
        {
            var plan = _planCache.GetOrAdd(viewModelType, BuildActivatorPlan);
            return plan.BusinessParameterType;
        }

        /// <summary>
        /// 构建激活器计划
        /// </summary>
        private ActivatorPlan BuildActivatorPlan(Type viewModelType)
        {
            var ctor = SelectConstructor(viewModelType);
            var parameters = ctor.GetParameters();

            // 无参构造
            if (parameters.Length == 0)
            {
                return BuildParameterlessActivator(viewModelType, ctor);
            }

            // 分析参数类型
            var paramInfos = AnalyzeParameters(parameters);

            // 构建编译表达式
            return BuildCompiledActivator(viewModelType, ctor, paramInfos);
        }

        /// <summary>
        /// 选择最合适的构造函数
        /// 优先级：最多参数的公共构造函数（支持 DI 注入更多依赖）
        /// </summary>
        private static ConstructorInfo SelectConstructor(Type viewModelType)
        {
            var ctors = viewModelType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (ctors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"类型 {viewModelType.Name} 没有公共构造函数");
            }

            // 选择参数最多的构造函数（支持更丰富的 DI）
            return ctors.OrderByDescending(c => c.GetParameters().Length).First();
        }

        /// <summary>
        /// 分析构造函数参数，区分服务依赖和业务参数
        /// </summary>
        private static List<ParameterAnalysis> AnalyzeParameters(ParameterInfo[] parameters)
        {
            var result = new List<ParameterAnalysis>(parameters.Length);
            Type? foundBusinessParam = null;

            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                var isService = IsServiceType(paramType);

                if (!isService)
                {
                    // 只允许一个业务参数
                    if (foundBusinessParam != null)
                    {
                        throw new InvalidOperationException(
                            $"构造函数存在多个业务参数: {foundBusinessParam.Name} 和 {paramType.Name}。" +
                            $"服务依赖应以 'I' 开头的接口类型。");
                    }
                    foundBusinessParam = paramType;
                }

                result.Add(new ParameterAnalysis
                {
                    ParameterInfo = param,
                    IsService = isService
                });
            }

            return result;
        }

        /// <summary>
        /// 判断类型是否为服务依赖（从 DI 容器解析）
        /// 规则：接口类型且以 'I' 开头
        /// </summary>
        private static bool IsServiceType(Type type)
        {
            // 接口类型且以 'I' 开头视为服务依赖
            if (type.IsInterface && type.Name.StartsWith('I'))
                return true;

            // 抽象类也可能是服务
            if (type.IsAbstract && type.IsClass)
                return true;

            return false;
        }

        /// <summary>
        /// 构建无参构造的激活器
        /// </summary>
        private static ActivatorPlan BuildParameterlessActivator(Type viewModelType, ConstructorInfo ctor)
        {
            // 编译：() => new TViewModel()
            var newExpr = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<BaseComponentViewModel>>(newExpr);
            var compiled = lambda.Compile();

            return new ActivatorPlan
            {
                BusinessParameterType = null,
                Activate = (_, _) => compiled()
            };
        }

        /// <summary>
        /// 构建带参数的编译表达式激活器
        /// </summary>
        private static ActivatorPlan BuildCompiledActivator(
            Type viewModelType,
            ConstructorInfo ctor,
            List<ParameterAnalysis> paramInfos)
        {
            // 找出业务参数类型
            var businessParam = paramInfos.FirstOrDefault(p => !p.IsService);
            var businessParamType = businessParam?.ParameterInfo.ParameterType;

            // 构建表达式参数
            var containerProviderParam = Expression.Parameter(typeof(IContainerProvider), "container");
            var businessObjParam = Expression.Parameter(typeof(object), "businessParam");

            var ctorArgs = new Expression[paramInfos.Count];

            for (int i = 0; i < paramInfos.Count; i++)
            {
                var info = paramInfos[i];
                var paramType = info.ParameterInfo.ParameterType;

                if (info.IsService)
                {
                    // 服务依赖：container.Resolve(paramType)
                    var resolveMethod = typeof(IContainerProvider)
                        .GetMethod(nameof(IContainerProvider.Resolve), [typeof(Type)])!;

                    var resolveCall = Expression.Call(
                        containerProviderParam,
                        resolveMethod,
                        Expression.Constant(paramType));

                    ctorArgs[i] = Expression.Convert(resolveCall, paramType);
                }
                else
                {
                    // 业务参数：转换传入的 object
                    if (businessParamType != null)
                    {
                        // 先进行类型转换
                        var convertCall = Expression.Call(
                            typeof(ComponentActivator),
                            nameof(ConvertBusinessParameter),
                            Type.EmptyTypes,
                            businessObjParam,
                            Expression.Constant(businessParamType));

                        ctorArgs[i] = Expression.Convert(convertCall, paramType);
                    }
                    else
                    {
                        ctorArgs[i] = Expression.Default(paramType);
                    }
                }
            }

            // 构建 new TViewModel(arg1, arg2, ...)
            var newExpr = Expression.New(ctor, ctorArgs);

            // 编译 Lambda
            var lambda = Expression.Lambda<Func<IContainerProvider, object?, BaseComponentViewModel>>(
                newExpr,
                containerProviderParam,
                businessObjParam);

            var compiled = lambda.Compile();

            return new ActivatorPlan
            {
                BusinessParameterType = businessParamType,
                Activate = compiled
            };
        }

        /// <summary>
        /// 转换业务参数（在表达式中调用）
        /// </summary>
        public static object? ConvertBusinessParameter(object? input, Type targetType)
        {
            return StringPrserCache.ConvertToTarget(input, targetType);
        }

        #region Nested Types

        private sealed class ParameterAnalysis
        {
            public ParameterInfo ParameterInfo { get; init; } = null!;
            public bool IsService { get; init; }
        }

        private sealed class ActivatorPlan
        {
            public Type? BusinessParameterType { get; init; }
            public Func<IContainerProvider, object?, BaseComponentViewModel> Activate { get; init; } = null!;
        }

        #endregion
    }
}
