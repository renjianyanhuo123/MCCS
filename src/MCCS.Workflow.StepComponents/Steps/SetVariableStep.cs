using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps
{
    /// <summary>
    /// 变量赋值步骤 - 设置工作流变量
    /// </summary>
    [StepComponent("set-variable", "设置变量",
        Description = "设置工作流变量值，可用于在步骤间传递数据",
        Category = ComponentCategory.DataProcessing,
        Icon = "Variable",
        Tags = new[] { "变量", "赋值", "数据", "设置" })]
    public class SetVariableStep : BaseWorkflowStep
    {
        [StepInput("VariableName")]
        public string VariableName { get; set; } = string.Empty;

        [StepInput("VariableValue")]
        public string VariableValue { get; set; } = string.Empty;

        [StepInput("ValueType")]
        public string ValueType { get; set; } = "string";

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
                Order = 3
            };
        }

        protected override Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var variableName = GetParameter<string>("VariableName") ?? string.Empty;
            var variableValue = GetParameter<string>("VariableValue") ?? string.Empty;
            var valueType = GetParameter<string>("ValueType") ?? "string";

            // 替换变量引用
            variableValue = context.ReplaceVariables(variableValue);

            // 类型转换
            object? typedValue = ConvertValue(variableValue, valueType);

            // 设置变量
            context.SetVariable(variableName, typedValue);

            context.Log($"设置变量 {variableName} = {typedValue}");

            return Task.FromResult(StepResult.Succeed(new Dictionary<string, object?>
            {
                ["VariableName"] = variableName,
                ["VariableValue"] = typedValue,
                ["ValueType"] = valueType
            }));
        }

        private static object? ConvertValue(string value, string valueType)
        {
            return valueType switch
            {
                "integer" => int.TryParse(value, out var intVal) ? intVal : 0,
                "double" => double.TryParse(value, out var doubleVal) ? doubleVal : 0.0,
                "boolean" => bool.TryParse(value, out var boolVal) && boolVal,
                "json" => value,
                _ => value
            };
        }
    }
}
