using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Components
{
    /// <summary>
    /// 延时组件 - 等待指定时间后继续执行
    /// </summary>
    [StepComponent("delay", "延时等待",
        Description = "等待指定的时间后继续执行下一步",
        Category = ComponentCategory.FlowControl,
        Icon = "TimerSand",
        Tags = new[] { "等待", "延时", "暂停", "计时" })]
    public class DelayComponent : BaseStepComponent
    {
        /// <summary>
        /// 延时时间（毫秒）
        /// </summary>
        public int DelayMilliseconds { get; set; } = 1000;

        /// <summary>
        /// 延时类型
        /// </summary>
        public string DelayType { get; set; } = "milliseconds";

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
                MaxValue = 86400000, // 最长24小时
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

        protected override async Task<ComponentExecutionResult> ExecuteCoreAsync(
            ComponentExecutionContext context,
            CancellationToken cancellationToken)
        {
            var delayValue = GetParameterValue<int>("DelayValue");
            var delayType = GetParameterValue<string>("DelayType") ?? "seconds";

            var delayMs = delayType switch
            {
                "milliseconds" => delayValue,
                "seconds" => delayValue * 1000,
                "minutes" => delayValue * 60 * 1000,
                "hours" => delayValue * 60 * 60 * 1000,
                _ => delayValue * 1000
            };

            context.Log?.Invoke($"开始等待 {delayValue} {GetUnitDisplayName(delayType)}", LogLevel.Info);

            // 使用 Task.Delay 实现可取消的等待
            await Task.Delay(delayMs, cancellationToken);

            context.Log?.Invoke("延时等待完成", LogLevel.Info);

            return ComponentExecutionResult.Success(new Dictionary<string, object?>
            {
                ["DelayedMs"] = delayMs
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

        public override IStepComponent Clone()
        {
            var clone = new DelayComponent();
            clone.SetParameterValues(GetParameterValues());
            return clone;
        }
    }
}
