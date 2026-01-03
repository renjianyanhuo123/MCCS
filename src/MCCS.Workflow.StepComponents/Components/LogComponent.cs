using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Components
{
    /// <summary>
    /// 日志组件 - 输出日志信息
    /// </summary>
    [StepComponent("log", "日志输出",
        Description = "输出日志信息，支持变量替换",
        Category = ComponentCategory.General,
        Icon = "TextBoxOutline",
        Tags = new[] { "日志", "输出", "调试", "打印" })]
    public class LogComponent : BaseStepComponent
    {
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
                Options = new List<SelectOption>
                {
                    new("Debug", "调试"),
                    new("Info", "信息"),
                    new("Warning", "警告"),
                    new("Error", "错误")
                },
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

        protected override Task<ComponentExecutionResult> ExecuteCoreAsync(
            ComponentExecutionContext context,
            CancellationToken cancellationToken)
        {
            var message = GetParameterValue<string>("Message") ?? string.Empty;
            var logLevelStr = GetParameterValue<string>("LogLevel") ?? "Info";
            var includeTimestamp = GetParameterValue<bool>("IncludeTimestamp");

            // 替换变量
            message = ReplaceVariables(message, context);

            // 添加时间戳
            if (includeTimestamp)
            {
                message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            }

            // 解析日志级别
            var logLevel = logLevelStr switch
            {
                "Debug" => LogLevel.Debug,
                "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                _ => LogLevel.Info
            };

            // 输出日志
            context.Log?.Invoke(message, logLevel);

            return Task.FromResult(ComponentExecutionResult.Success(new Dictionary<string, object?>
            {
                ["LoggedMessage"] = message,
                ["LogLevel"] = logLevelStr
            }));
        }

        private static string ReplaceVariables(string template, ComponentExecutionContext context)
        {
            // 简单的变量替换实现 ${variableName}
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
            var clone = new LogComponent();
            clone.SetParameterValues(GetParameterValues());
            return clone;
        }
    }
}
