using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 预加载/预压步骤
    /// 入参：预载目标（力/位移/应变）、速率、保持时间、回退策略
    /// 出参：预载曲线摘要（峰值、残余位移、是否滑移报警）
    /// </summary>
    [StepComponent("preload", "预加载",
        Description = "执行预加载（消隙、就位、检查滑移）",
        Category = ComponentCategory.ManualOperation,
        Icon = "ArrowUpBoldCircleOutline",
        Tags = ["预载", "预压", "消隙", "就位"])]
    public class PreloadStep : BaseWorkflowStep
    {
        [StepInput("TargetValue")]
        public double TargetValue { get; set; }

        [StepInput("ControlMode")]
        public string ControlModeStr { get; set; } = "Force";

        [StepInput("Rate")]
        public double Rate { get; set; } = 1.0;

        [StepInput("HoldSeconds")]
        public int HoldSeconds { get; set; } = 10;

        [StepInput("ReturnToZero")]
        public bool ReturnToZero { get; set; } = false;

        [StepInput("SlipThreshold")]
        public double SlipThreshold { get; set; } = 0.5;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new DoubleParameter
            {
                Name = "TargetValue",
                DisplayName = "预载目标值",
                Description = "预载的目标值（力或位移）",
                IsRequired = true,
                DefaultValue = 5.0,
                Order = 1
            };

            yield return new SelectParameter
            {
                Name = "ControlMode",
                DisplayName = "控制模式",
                Description = "预载的控制模式",
                IsRequired = true,
                DefaultValue = "Force",
                Options =
                [
                    new SelectOption("Force", "力控制"),
                    new SelectOption("Displacement", "位移控制")
                ],
                Order = 2
            };

            yield return new DoubleParameter
            {
                Name = "Rate",
                DisplayName = "速率",
                Description = "预载速率（kN/s 或 mm/s）",
                IsRequired = true,
                DefaultValue = 1.0,
                MinValue = 0.01,
                Order = 3
            };

            yield return new IntegerParameter
            {
                Name = "HoldSeconds",
                DisplayName = "保持时间(秒)",
                Description = "到达目标后的保持时间",
                IsRequired = true,
                DefaultValue = 10,
                MinValue = 0,
                MaxValue = 300,
                Order = 4
            };

            yield return new BooleanParameter
            {
                Name = "ReturnToZero",
                DisplayName = "保持后回零",
                Description = "保持后是否卸载回零",
                DefaultValue = false,
                Order = 5
            };

            yield return new DoubleParameter
            {
                Name = "SlipThreshold",
                DisplayName = "滑移阈值(mm)",
                Description = "判定滑移的位移突变阈值",
                IsRequired = true,
                DefaultValue = 0.5,
                MinValue = 0.01,
                Order = 6
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var targetValue = GetParameter<double>("TargetValue");
            var controlModeStr = GetParameter<string>("ControlMode") ?? "Force";
            var rate = GetParameter<double>("Rate");
            var holdSeconds = GetParameter<int>("HoldSeconds");
            var returnToZero = GetParameter<bool>("ReturnToZero");
            var slipThreshold = GetParameter<double>("SlipThreshold");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var controlMode = controlModeStr == "Displacement" ? ControlMode.Displacement : ControlMode.Force;

            testRunContext.AddEvent("PreloadStep", $"开始预载: 目标 {targetValue}, 模式 {controlMode}, 速率 {rate}");

            // TODO: 向站点服务发送预载指令
            // var preloadCommand = new PreloadCommand
            // {
            //     TargetValue = targetValue,
            //     ControlMode = controlMode,
            //     Rate = rate,
            //     HoldSeconds = holdSeconds,
            //     SlipThreshold = slipThreshold
            // };
            // var result = await _stationService.ExecutePreloadAsync(preloadCommand, context.CancellationToken);

            // 模拟预载执行
            var rampTime = Math.Abs(targetValue) / rate;
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(rampTime, 5)), context.CancellationToken);

            // 模拟保持
            if (holdSeconds > 0)
            {
                testRunContext.AddEvent("PreloadStep", $"预载到达目标，保持 {holdSeconds} 秒");
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(holdSeconds, 5)), context.CancellationToken);
            }

            // 模拟预载结果
            var preloadResult = new PreloadResult
            {
                Success = true,
                PeakForce = controlMode == ControlMode.Force ? targetValue : targetValue * 0.1,
                PeakDisplacement = controlMode == ControlMode.Displacement ? targetValue : targetValue * 10,
                ResidualDisplacement = 0.02,
                SlipAlarm = false
            };

            // 回零（如果需要）
            if (returnToZero)
            {
                testRunContext.AddEvent("PreloadStep", "预载后卸载回零");
                // TODO: await _stationService.UnloadToZeroAsync(rate);
                await Task.Delay(TimeSpan.FromSeconds(2), context.CancellationToken);
            }

            // 记录到上下文
            testRunContext.SetStepOutput("PreloadResult", preloadResult);
            testRunContext.AddEvent("PreloadStep",
                $"预载完成, 峰值力: {preloadResult.PeakForce:F2}, 残余位移: {preloadResult.ResidualDisplacement:F3}");

            if (preloadResult.SlipAlarm)
            {
                testRunContext.AddEvent("PreloadStep", "检测到滑移！", EventLevel.Warning);
            }

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = preloadResult.SlipAlarm ? StepResultCode.Warning : StepResultCode.OK,
                ["PeakForce"] = preloadResult.PeakForce,
                ["PeakDisplacement"] = preloadResult.PeakDisplacement,
                ["ResidualDisplacement"] = preloadResult.ResidualDisplacement,
                ["SlipAlarm"] = preloadResult.SlipAlarm
            });
        }
    }
}
