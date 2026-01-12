using MCCS.Workflow.Contact.Models;
using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps
{
    /// <summary>
    /// 分支步骤 - 根据数据值将流程导向不同分支
    /// 支持多条件分支控制，类似于 switch-case 语句
    /// </summary>
    [StepComponent("branch", "分支", NodeDisplayTypeEnum.Decision,
        Description = "根据数据值选择执行的分支，支持多条件分支控制",
        Category = ComponentCategory.DataProcessing,
        Icon = "SourceBranch", 
        Tags = ["分支", "数据处理", "switch", "路由", "多分支"])]
    public class BranchStep : BaseWorkflowStep
    {
        /// <summary>
        /// 要评估的表达式或变量
        /// </summary>
        [StepInput("Expression")]
        public string Expression { get; set; } = string.Empty;

        /// <summary>
        /// 分支映射列表（值 -> 目标步骤）
        /// </summary>
        [StepInput("BranchMappings")]
        public List<KeyValueItem>? BranchMappings { get; set; }

        /// <summary>
        /// 默认分支（当没有匹配时跳转）
        /// </summary>
        [StepInput("DefaultBranch")]
        public string? DefaultBranch { get; set; }

        /// <summary>
        /// 是否区分大小写
        /// </summary>
        [StepInput("CaseSensitive")]
        public bool CaseSensitive { get; set; } = false;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "Expression",
                DisplayName = "分支表达式",
                Description = "要评估的表达式或变量，支持变量引用 ${变量名}，将根据其值决定进入哪个分支",
                IsRequired = true,
                Placeholder = "${dataType} 或 ${status}",
                Order = 1
            };

            yield return new KeyValueListParameter
            {
                Name = "BranchMappings",
                DisplayName = "分支映射",
                Description = "定义值与目标步骤的映射关系：当表达式值匹配键时，跳转到对应的步骤",
                IsRequired = true,
                KeyLabel = "匹配值",
                ValueLabel = "目标步骤",
                AllowAdd = true,
                AllowDelete = true,
                AllowDuplicateKeys = false,
                Order = 2
            };

            yield return new StringParameter
            {
                Name = "DefaultBranch",
                DisplayName = "默认分支",
                Description = "当表达式值不匹配任何分支时，跳转到此步骤（可选，若不设置则继续执行下一步）",
                Placeholder = "默认目标步骤名称",
                Order = 3,
                Group = "高级设置"
            };

            yield return new BooleanParameter
            {
                Name = "CaseSensitive",
                DisplayName = "区分大小写",
                Description = "匹配分支值时是否区分大小写",
                DefaultValue = false,
                Order = 4,
                Group = "高级设置"
            };
        }

        protected override Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            // 获取参数值
            var expression = GetParameter<string>("Expression") ?? string.Empty;
            var mappings = GetParameter<List<KeyValueItem>>("BranchMappings");
            var defaultBranch = GetParameter<string>("DefaultBranch");
            var caseSensitive = GetParameter<bool>("CaseSensitive");

            // 替换变量，获取实际值
            var expressionValue = context.ReplaceVariables(expression);

            // 准备输出数据
            var output = new Dictionary<string, object?>
            {
                ["Expression"] = expression,
                ["ExpressionValue"] = expressionValue,
                ["CaseSensitive"] = caseSensitive
            };

            // 如果没有配置分支映射，返回错误
            if (mappings == null || mappings.Count == 0)
            {
                return Task.FromResult(StepResult.Fail("未配置分支映射"));
            }

            // 设置字符串比较方式
            var comparison = caseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            // 查找匹配的分支
            var matchedBranch = mappings
                .Where(m => m.IsEnabled)
                .FirstOrDefault(m => string.Equals(m.Key, expressionValue, comparison));

            if (matchedBranch != null)
            {
                // 找到匹配的分支
                output["MatchedValue"] = matchedBranch.Key;
                output["TargetBranch"] = matchedBranch.Value;
                output["BranchType"] = "matched";

                return Task.FromResult(StepResult.Branch(matchedBranch.Value, output));
            }

            // 没有找到匹配，检查是否有默认分支
            if (!string.IsNullOrEmpty(defaultBranch))
            {
                output["MatchedValue"] = null;
                output["TargetBranch"] = defaultBranch;
                output["BranchType"] = "default";

                return Task.FromResult(StepResult.Branch(defaultBranch, output));
            }

            // 没有匹配的分支也没有默认分支，继续执行下一步
            output["MatchedValue"] = null;
            output["TargetBranch"] = null;
            output["BranchType"] = "none";
            output["Message"] = $"表达式值 '{expressionValue}' 未匹配任何分支，将继续执行下一步";

            return Task.FromResult(StepResult.Succeed(output));
        }

        protected override ComponentValidationResult ValidateCustom()
        {
            var result = ComponentValidationResult.Valid();

            // 验证分支映射中的目标步骤不能为空
            var mappings = GetParameter<List<KeyValueItem>>("BranchMappings");
            if (mappings != null)
            {
                var emptyTargets = mappings
                    .Where(m => m.IsEnabled && string.IsNullOrWhiteSpace(m.Value))
                    .Select(m => m.Key)
                    .ToList();

                if (emptyTargets.Any())
                {
                    result.AddError("BranchMappings",
                        $"以下分支的目标步骤为空: {string.Join(", ", emptyTargets)}");
                }
            }

            return result;
        }
    }
}
