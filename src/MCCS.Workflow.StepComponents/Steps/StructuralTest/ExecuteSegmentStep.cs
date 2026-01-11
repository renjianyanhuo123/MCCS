using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 单段执行步骤（斜坡/台阶/保持）
    /// 入参：Segment（目标、速率、保持、控制模式）、限值、采样策略
    /// 出参：该段统计 Max/Min/EndValue、段内事件
    /// </summary>
    [StepComponent("execute-segment", "执行控制段",
        Description = "执行单个控制段（斜坡/台阶/保持）",
        Category = ComponentCategory.ControlExecution,
        Icon = "ChartLineVariant",
        Tags = ["执行", "控制", "斜坡", "保持", "段"])]
    public class ExecuteSegmentStep : BaseWorkflowStep
    {
        [StepInput("SegmentType")]
        public string SegmentType { get; set; } = "Ramp";

        [StepInput("Target")]
        public double Target { get; set; }

        [StepInput("Rate")]
        public double Rate { get; set; } = 1.0;

        [StepInput("ControlMode")]
        public string ControlModeStr { get; set; } = "Displacement";

        [StepInput("HoldSeconds")]
        public double HoldSeconds { get; set; } = 0;

        [StepInput("Tolerance")]
        public double Tolerance { get; set; } = 0.01;

        [StepInput("TimeoutSeconds")]
        public double TimeoutSeconds { get; set; } = 300;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new SelectParameter
            {
                Name = "SegmentType",
                DisplayName = "段类型",
                Description = "控制段类型",
                IsRequired = true,
                DefaultValue = "Ramp",
                Options =
                [
                    new SelectOption("Ramp", "斜坡"),
                    new SelectOption("Hold", "保持"),
                    new SelectOption("Step", "台阶")
                ],
                Order = 1
            };

            yield return new DoubleParameter
            {
                Name = "Target",
                DisplayName = "目标值",
                Description = "斜坡/台阶的目标值",
                IsRequired = true,
                Order = 2
            };

            yield return new DoubleParameter
            {
                Name = "Rate",
                DisplayName = "速率",
                Description = "变化速率（单位/秒）",
                IsRequired = true,
                DefaultValue = 1.0,
                MinValue = 0.001,
                Order = 3
            };

            yield return new SelectParameter
            {
                Name = "ControlMode",
                DisplayName = "控制模式",
                Description = "控制模式",
                IsRequired = true,
                DefaultValue = "Displacement",
                Options =
                [
                    new SelectOption("Force", "力控制"),
                    new SelectOption("Displacement", "位移控制"),
                    new SelectOption("Strain", "应变控制")
                ],
                Order = 4
            };

            yield return new DoubleParameter
            {
                Name = "HoldSeconds",
                DisplayName = "保持时间(秒)",
                Description = "到达目标后的保持时间",
                IsRequired = false,
                DefaultValue = 0,
                MinValue = 0,
                Order = 5
            };

            yield return new DoubleParameter
            {
                Name = "Tolerance",
                DisplayName = "到达容差",
                Description = "判定到达目标的容差",
                IsRequired = true,
                DefaultValue = 0.01,
                MinValue = 0.001,
                Order = 6
            };

            yield return new DoubleParameter
            {
                Name = "TimeoutSeconds",
                DisplayName = "超时时间(秒)",
                Description = "段执行的超时时间",
                IsRequired = true,
                DefaultValue = 300,
                MinValue = 1,
                Order = 7
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var segmentType = GetParameter<string>("SegmentType") ?? "Ramp";
            var target = GetParameter<double>("Target");
            var rate = GetParameter<double>("Rate");
            var controlModeStr = GetParameter<string>("ControlMode") ?? "Displacement";
            var holdSeconds = GetParameter<double>("HoldSeconds");
            var tolerance = GetParameter<double>("Tolerance");
            var timeoutSeconds = GetParameter<double>("TimeoutSeconds");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var controlMode = controlModeStr switch
            {
                "Force" => ControlMode.Force,
                "Strain" => ControlMode.Strain,
                _ => ControlMode.Displacement
            };

            var segmentId = Guid.NewGuid().ToString();
            var startTime = DateTime.Now;
            var result = new SegmentExecutionResult
            {
                SegmentId = segmentId,
                Success = true
            };

            testRunContext.AddEvent("ExecuteSegmentStep",
                $"开始执行段: {segmentType}, 目标: {target}, 模式: {controlMode}, 速率: {rate}");

            // TODO: 向站点服务发送段执行指令
            // var segmentCommand = new SegmentCommand
            // {
            //     SegmentType = segmentType,
            //     Target = target,
            //     Rate = rate,
            //     ControlMode = controlMode,
            //     Tolerance = tolerance,
            //     TimeoutSeconds = timeoutSeconds
            // };
            //
            // using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            // cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            //
            // var execResult = await _stationService.ExecuteSegmentAsync(segmentCommand, cts.Token);

            // 模拟段执行
            var estimatedTime = segmentType switch
            {
                "Hold" => holdSeconds,
                "Ramp" => Math.Abs(target - testRunContext.Measurement.Displacement) / rate,
                _ => 2.0
            };

            // 模拟执行时间（限制最大等待时间）
            var waitTime = Math.Min(estimatedTime, 5);
            await Task.Delay(TimeSpan.FromSeconds(waitTime), context.CancellationToken);

            // 保持阶段
            if (holdSeconds > 0 && segmentType != "Hold")
            {
                testRunContext.AddEvent("ExecuteSegmentStep", $"到达目标，保持 {holdSeconds} 秒");
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(holdSeconds, 3)), context.CancellationToken);
            }

            // 模拟结果
            result.MaxValue = target * 1.01;
            result.MinValue = target * 0.99;
            result.EndValue = target;
            result.DurationSeconds = (DateTime.Now - startTime).TotalSeconds;
            result.LimitTriggered = false;

            // 更新测量快照
            if (controlMode == ControlMode.Displacement)
            {
                testRunContext.Measurement.Displacement = target;
            }
            else if (controlMode == ControlMode.Force)
            {
                testRunContext.Measurement.Force = target;
            }

            // 更新当前段索引
            testRunContext.CurrentSegmentIndex++;

            testRunContext.AddEvent("ExecuteSegmentStep",
                $"段执行完成, 最大值: {result.MaxValue:F3}, 最小值: {result.MinValue:F3}, 耗时: {result.DurationSeconds:F1}s");

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = result.LimitTriggered ? StepResultCode.Warning : StepResultCode.OK,
                ["SegmentId"] = segmentId,
                ["SegmentType"] = segmentType,
                ["MaxValue"] = result.MaxValue,
                ["MinValue"] = result.MinValue,
                ["EndValue"] = result.EndValue,
                ["DurationSeconds"] = result.DurationSeconds,
                ["LimitTriggered"] = result.LimitTriggered
            });
        }
    }
}
