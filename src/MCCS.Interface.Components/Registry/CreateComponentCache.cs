using System.Collections.Concurrent;
using System.Reflection;

using MCCS.Interface.Components.ViewModels;

namespace MCCS.Interface.Components.Registry
{
    public static class CreateComponentCache
    {
        private static readonly ConcurrentDictionary<Type, CtorPlan> _ctorPlanCache = new();

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

        public static BaseComponentViewModel CreateInstanceWithParameter(Type viewModelType, object? parameter)
        {
            var plan = GetCtorPlan(viewModelType);

            // 无参构造
            if (plan.ParamType == null)
            {
                // 如果传了 parameter，你可以选择忽略或报错。这里我选择“忽略”更宽松。
                return (BaseComponentViewModel)plan.Ctor.Invoke(null);
            }

            // 单参构造
            var argType = plan.ParamType;

            var arg = StringPrserCache.ConvertToTarget(parameter, argType);

            // 如果 arg 为 null，但参数类型不可空值类型 -> 报错更明确
            if (arg == null && argType.IsValueType && Nullable.GetUnderlyingType(argType) == null)
            {
                throw new InvalidOperationException(
                    $"创建 {viewModelType.Name} 需要 {argType.Name} 参数，但传入为 null");
            }

            return (BaseComponentViewModel)plan.Ctor.Invoke([arg]);
        }

    }
}
