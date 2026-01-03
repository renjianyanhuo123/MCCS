using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Components
{
    /// <summary>
    /// 条件判断组件 - 根据条件决定执行流程
    /// </summary>
    [StepComponent("condition", "条件判断",
        Description = "根据条件表达式决定执行流程",
        Category = ComponentCategory.FlowControl,
        Icon = "CallSplit",
        Tags = new[] { "条件", "判断", "分支", "if" })]
    public class ConditionComponent : BaseStepComponent
    {
        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "LeftOperand",
                DisplayName = "左操作数",
                Description = "条件表达式的左操作数，支持变量引用 ${变量名}",
                IsRequired = true,
                Placeholder = "${status}",
                Order = 1
            };

            yield return new SelectParameter
            {
                Name = "Operator",
                DisplayName = "比较运算符",
                Description = "条件比较的运算符",
                IsRequired = true,
                DefaultValue = "equals",
                Options = new List<SelectOption>
                {
                    new("equals", "等于 (==)"),
                    new("not_equals", "不等于 (!=)"),
                    new("greater_than", "大于 (>)"),
                    new("greater_or_equal", "大于等于 (>=)"),
                    new("less_than", "小于 (<)"),
                    new("less_or_equal", "小于等于 (<=)"),
                    new("contains", "包含"),
                    new("not_contains", "不包含"),
                    new("starts_with", "以...开头"),
                    new("ends_with", "以...结尾"),
                    new("is_empty", "为空"),
                    new("is_not_empty", "不为空"),
                    new("is_null", "为null"),
                    new("is_not_null", "不为null")
                },
                Order = 2
            };

            yield return new StringParameter
            {
                Name = "RightOperand",
                DisplayName = "右操作数",
                Description = "条件表达式的右操作数（某些运算符不需要）",
                Placeholder = "比较的值",
                Order = 3
            };

            yield return new BooleanParameter
            {
                Name = "CaseSensitive",
                DisplayName = "区分大小写",
                Description = "字符串比较时是否区分大小写",
                DefaultValue = false,
                Order = 4
            };
        }

        protected override Task<ComponentExecutionResult> ExecuteCoreAsync(
            ComponentExecutionContext context,
            CancellationToken cancellationToken)
        {
            var leftOperand = GetParameterValue<string>("LeftOperand") ?? string.Empty;
            var op = GetParameterValue<string>("Operator") ?? "equals";
            var rightOperand = GetParameterValue<string>("RightOperand") ?? string.Empty;
            var caseSensitive = GetParameterValue<bool>("CaseSensitive");

            // 替换变量
            leftOperand = ReplaceVariables(leftOperand, context);
            rightOperand = ReplaceVariables(rightOperand, context);

            var result = EvaluateCondition(leftOperand, op, rightOperand, caseSensitive);

            context.Log?.Invoke($"条件判断: '{leftOperand}' {op} '{rightOperand}' = {result}", LogLevel.Info);

            return Task.FromResult(ComponentExecutionResult.Success(new Dictionary<string, object?>
            {
                ["Result"] = result,
                ["LeftValue"] = leftOperand,
                ["RightValue"] = rightOperand,
                ["Operator"] = op
            }));
        }

        private static bool EvaluateCondition(string left, string op, string right, bool caseSensitive)
        {
            var comparison = caseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            return op switch
            {
                "equals" => string.Equals(left, right, comparison),
                "not_equals" => !string.Equals(left, right, comparison),
                "greater_than" => CompareNumbers(left, right) > 0,
                "greater_or_equal" => CompareNumbers(left, right) >= 0,
                "less_than" => CompareNumbers(left, right) < 0,
                "less_or_equal" => CompareNumbers(left, right) <= 0,
                "contains" => left.Contains(right, comparison),
                "not_contains" => !left.Contains(right, comparison),
                "starts_with" => left.StartsWith(right, comparison),
                "ends_with" => left.EndsWith(right, comparison),
                "is_empty" => string.IsNullOrEmpty(left),
                "is_not_empty" => !string.IsNullOrEmpty(left),
                "is_null" => left == null,
                "is_not_null" => left != null,
                _ => false
            };
        }

        private static int CompareNumbers(string left, string right)
        {
            if (double.TryParse(left, out var leftNum) && double.TryParse(right, out var rightNum))
            {
                return leftNum.CompareTo(rightNum);
            }
            return string.Compare(left, right, StringComparison.Ordinal);
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

            foreach (var kvp in context.PreviousStepOutput)
            {
                result = result.Replace($"${{prev.{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        public override IStepComponent Clone()
        {
            var clone = new ConditionComponent();
            clone.SetParameterValues(GetParameterValues());
            return clone;
        }
    }
}
