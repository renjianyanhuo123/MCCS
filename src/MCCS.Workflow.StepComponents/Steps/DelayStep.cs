using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps
{
    /// <summary>
    /// 延时步骤 - 等待指定时间后继续执行
    /// </summary>
    [StepComponent("delay", "延时等待",
        Description = "等待指定的时间后继续执行下一步",
        Category = ComponentCategory.FlowControl,
        Icon = "TimerSand",
        Tags = new[] { "等待", "延时", "暂停", "计时" })]
    public class DelayStep : BaseWorkflowStep
    {
        /// <summary>
        /// 延时数值
        /// </summary>
        [StepInput("DelayValue")]
        public int DelayValue { get; set; } = 1;

        /// <summary>
        /// 延时类型（milliseconds, seconds, minutes, hours）
        /// </summary>
        [StepInput("DelayType")]
        public string DelayType { get; set; } = "seconds";

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new IntegerParameter
            {
                Name = "DelayValue",
                DisplayName = "延时数值",
                Description = "等待的时间数值",
                IsRequired = true,
                DefaultValue = 1,
                MinValue = 0,
                MaxValue = 86400000,
                Order = 1
            };

            yield return new SelectParameter
            {
                Name = "DelayType",
                DisplayName = "时间单位",
                Description = "延时的时间单位",
                IsRequired = true,
                DefaultValue = "seconds",
                Options = new List<SelectOption>
                {
                    new("milliseconds", "毫秒"),
                    new("seconds", "秒"),
                    new("minutes", "分钟"),
                    new("hours", "小时")
                },
                Order = 2
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var delayValue = GetParameter<int>("DelayValue");
            var delayType = GetParameter<string>("DelayType") ?? "seconds";

            var delayMs = delayType switch
            {
                "milliseconds" => delayValue,
                "seconds" => delayValue * 1000,
                "minutes" => delayValue * 60 * 1000,
                "hours" => delayValue * 60 * 60 * 1000,
                _ => delayValue * 1000
            };

            context.Log($"开始等待 {delayValue} {GetUnitDisplayName(delayType)}");

            await Task.Delay(delayMs, context.CancellationToken);

            context.Log("延时等待完成");

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["DelayedMs"] = delayMs,
                ["DelayValue"] = delayValue,
                ["DelayUnit"] = delayType
            });
        }

        private static string GetUnitDisplayName(string unit) => unit switch
        {
            "milliseconds" => "毫秒",
            "seconds" => "秒",
            "minutes" => "分钟",
            "hours" => "小时",
            _ => "秒"
        };
    }
}
