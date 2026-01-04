using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps
{
    /// <summary>
    /// 条件判断步骤 - 根据条件决定执行流程
    /// </summary>
    [StepComponent("condition", "条件判断",
        Description = "根据条件表达式决定执行流程",
        Category = ComponentCategory.FlowControl,
        Icon = "CallSplit",
        Tags = new[] { "条件", "判断", "分支", "if" })]
    public class ConditionStep : BaseWorkflowStep
    {
        [StepInput("LeftOperand")]
        public string LeftOperand { get; set; } = string.Empty;

        [StepInput("Operator")]
        public string Operator { get; set; } = "equals";

        [StepInput("RightOperand")]
        public string RightOperand { get; set; } = string.Empty;

        [StepInput("CaseSensitive")]
        public bool CaseSensitive { get; set; } = false;

        [StepInput("TrueBranch")]
        public string? TrueBranch { get; set; }

        [StepInput("FalseBranch")]
        public string? FalseBranch { get; set; }

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
                Options =
                [
                    new SelectOption("equals", "等于 (==)"),
                    new SelectOption("not_equals", "不等于 (!=)"),
                    new SelectOption("greater_than", "大于 (>)"),
                    new SelectOption("greater_or_equal", "大于等于 (>=)"),
                    new SelectOption("less_than", "小于 (<)"),
                    new SelectOption("less_or_equal", "小于等于 (<=)"),
                    new SelectOption("contains", "包含"),
                    new SelectOption("not_contains", "不包含"),
                    new SelectOption("starts_with", "以...开头"),
                    new SelectOption("ends_with", "以...结尾"),
                    new SelectOption("is_empty", "为空"),
                    new SelectOption("is_not_empty", "不为空")
                ],
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

            yield return new StringParameter
            {
                Name = "TrueBranch",
                DisplayName = "条件为真时跳转",
                Description = "条件为真时跳转到的步骤名称（可选）",
                Placeholder = "步骤名称",
                Order = 5,
                Group = "分支设置"
            };

            yield return new StringParameter
            {
                Name = "FalseBranch",
                DisplayName = "条件为假时跳转",
                Description = "条件为假时跳转到的步骤名称（可选）",
                Placeholder = "步骤名称",
                Order = 6,
                Group = "分支设置"
            };
        }

        protected override Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var leftOperand = GetParameter<string>("LeftOperand") ?? string.Empty;
            var op = GetParameter<string>("Operator") ?? "equals";
            var rightOperand = GetParameter<string>("RightOperand") ?? string.Empty;
            var caseSensitive = GetParameter<bool>("CaseSensitive");
            var trueBranch = GetParameter<string>("TrueBranch");
            var falseBranch = GetParameter<string>("FalseBranch");

            // 替换变量
            leftOperand = context.ReplaceVariables(leftOperand);
            rightOperand = context.ReplaceVariables(rightOperand);

            var result = EvaluateCondition(leftOperand, op, rightOperand, caseSensitive);

            context.Log($"条件判断: '{leftOperand}' {op} '{rightOperand}' = {result}");

            var output = new Dictionary<string, object?>
            {
                ["Result"] = result,
                ["LeftValue"] = leftOperand,
                ["RightValue"] = rightOperand,
                ["Operator"] = op
            };

            // 如果有分支设置，返回分支结果
            if (result && !string.IsNullOrEmpty(trueBranch))
            {
                return Task.FromResult(StepResult.Branch(trueBranch, output));
            }
            else if (!result && !string.IsNullOrEmpty(falseBranch))
            {
                return Task.FromResult(StepResult.Branch(falseBranch, output));
            }

            return Task.FromResult(StepResult.Succeed(output));
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
    }
}
