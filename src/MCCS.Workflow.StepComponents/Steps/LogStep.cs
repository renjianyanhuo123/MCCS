using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps
{
    /// <summary>
    /// 日志步骤 - 输出日志信息
    /// </summary>
    [StepComponent("log", "日志输出",
        Description = "输出日志信息，支持变量替换",
        Category = ComponentCategory.General,
        Icon = "TextBoxOutline",
        Tags = ["日志", "输出", "调试", "打印"])]
    public class LogStep : BaseWorkflowStep
    {
        [StepInput("Message")]
        public string Message { get; set; } = string.Empty;

        [StepInput("LogLevel")]
        public string LogLevelValue { get; set; } = "Info";

        [StepInput("IncludeTimestamp")]
        public bool IncludeTimestamp { get; set; } = true;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new MultilineTextParameter
            {
                Name = "Message",
                DisplayName = "日志内容",
                Description = "要输出的日志内容，支持使用 ${变量名} 引用变量",
                IsRequired = true,
                Placeholder = "请输入日志内容...",
                Rows = 3,
                Order = 1
            };

            yield return new SelectParameter
            {
                Name = "LogLevel",
                DisplayName = "日志级别",
                Description = "日志的严重程度",
                IsRequired = true,
                DefaultValue = "Info",
                Options =
                [
                    new SelectOption("Debug", "调试"),
                    new SelectOption("Info", "信息"),
                    new SelectOption("Warning", "警告"),
                    new SelectOption("Error", "错误")
                ],
                Order = 2
            };

            yield return new BooleanParameter
            {
                Name = "IncludeTimestamp",
                DisplayName = "包含时间戳",
                Description = "是否在日志中包含时间戳",
                DefaultValue = true,
                Order = 3
            };
        }

        protected override Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var message = GetParameter<string>("Message") ?? string.Empty;
            var logLevelStr = GetParameter<string>("LogLevel") ?? "Info";
            var includeTimestamp = GetParameter<bool>("IncludeTimestamp");

            // 替换变量
            message = context.ReplaceVariables(message);

            // 添加时间戳
            if (includeTimestamp)
            {
                message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            }
             
            return Task.FromResult(StepResult.Succeed(new Dictionary<string, object?>
            {
                ["LoggedMessage"] = message,
                ["LogLevel"] = logLevelStr
            }));
        }
    }
}
