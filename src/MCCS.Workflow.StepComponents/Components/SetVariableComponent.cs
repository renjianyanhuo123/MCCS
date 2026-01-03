using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Components
{
    /// <summary>
    /// 变量赋值组件 - 设置工作流变量
    /// </summary>
    [StepComponent("set-variable", "设置变量",
        Description = "设置工作流变量值，可用于在步骤间传递数据",
        Category = ComponentCategory.DataProcessing,
        Icon = "Variable",
        Tags = new[] { "变量", "赋值", "数据", "设置" })]
    public class SetVariableComponent : BaseStepComponent
    {
        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "VariableName",
                DisplayName = "变量名称",
                Description = "要设置的变量名称",
                IsRequired = true,
                Placeholder = "例如: userId",
                ValidationPattern = @"^[a-zA-Z_][a-zA-Z0-9_]*$",
                ValidationMessage = "变量名必须以字母或下划线开头，只能包含字母、数字和下划线",
                Order = 1
            };

            yield return new StringParameter
            {
                Name = "VariableValue",
                DisplayName = "变量值",
                Description = "变量的值，支持使用 ${变量名} 引用其他变量",
                IsRequired = true,
                Placeholder = "请输入变量值",
                Order = 2
            };

            yield return new SelectParameter
            {
                Name = "VariableScope",
                DisplayName = "变量作用域",
                Description = "变量的作用范围",
                IsRequired = true,
                DefaultValue = "global",
                Options = new List<SelectOption>
                {
                    new("global", "全局变量（跨步骤共享）"),
                    new("local", "本地变量（仅当前步骤）")
                },
                Order = 3
            };

            yield return new SelectParameter
            {
                Name = "ValueType",
                DisplayName = "值类型",
                Description = "变量值的数据类型",
                DefaultValue = "string",
                Options = new List<SelectOption>
                {
                    new("string", "字符串"),
                    new("integer", "整数"),
                    new("double", "小数"),
                    new("boolean", "布尔值"),
                    new("json", "JSON对象")
                },
                Order = 4
            };
        }

        protected override Task<ComponentExecutionResult> ExecuteCoreAsync(
            ComponentExecutionContext context,
            CancellationToken cancellationToken)
        {
            var variableName = GetParameterValue<string>("VariableName") ?? string.Empty;
            var variableValue = GetParameterValue<string>("VariableValue") ?? string.Empty;
            var scope = GetParameterValue<string>("VariableScope") ?? "global";
            var valueType = GetParameterValue<string>("ValueType") ?? "string";

            // 替换变量引用
            variableValue = ReplaceVariables(variableValue, context);

            // 类型转换
            object? typedValue = ConvertValue(variableValue, valueType);

            // 设置变量
            if (scope == "global")
            {
                context.SetGlobalVariable(variableName, typedValue);
            }
            else
            {
                context.SetLocalVariable(variableName, typedValue);
            }

            context.Log?.Invoke($"设置{(scope == "global" ? "全局" : "本地")}变量 {variableName} = {typedValue}", LogLevel.Info);

            return Task.FromResult(ComponentExecutionResult.Success(new Dictionary<string, object?>
            {
                ["VariableName"] = variableName,
                ["VariableValue"] = typedValue,
                ["Scope"] = scope
            }));
        }

        private static string ReplaceVariables(string template, ComponentExecutionContext context)
        {
            var result = template;

            foreach (var kvp in context.GlobalVariables)
            {
                result = result.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            foreach (var kvp in context.LocalVariables)
            {
                result = result.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        private static object? ConvertValue(string value, string valueType)
        {
            return valueType switch
            {
                "integer" => int.TryParse(value, out var intVal) ? intVal : 0,
                "double" => double.TryParse(value, out var doubleVal) ? doubleVal : 0.0,
                "boolean" => bool.TryParse(value, out var boolVal) && boolVal,
                "json" => value, // JSON保持为字符串，实际使用时再解析
                _ => value
            };
        }

        public override IStepComponent Clone()
        {
            var clone = new SetVariableComponent();
            clone.SetParameterValues(GetParameterValues());
            return clone;
        }
    }
}
