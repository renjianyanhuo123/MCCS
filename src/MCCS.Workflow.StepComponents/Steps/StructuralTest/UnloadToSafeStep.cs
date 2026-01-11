using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 卸载与回零步骤
    /// 入参：卸载目标（0 或安全位移）、速率、保持
    /// 出参：卸载完成状态、残余值
    /// </summary>
    [StepComponent("unload-to-safe", "卸载回零",
        Description = "安全卸载到零位或安全位置",
        Category = ComponentCategory.ControlExecution,
        Icon = "ArrowDownBoldCircleOutline",
        Tags = ["卸载", "回零", "安全", "停机"])]
    public class UnloadToSafeStep : BaseWorkflowStep
    {
        [StepInput("TargetPosition")]
        public double TargetPosition { get; set; } = 0;

        [StepInput("ControlMode")]
        public string ControlModeStr { get; set; } = "Displacement";

        [StepInput("Rate")]
        public double Rate { get; set; } = 1.0;

        [StepInput("HoldSeconds")]
        public int HoldSeconds { get; set; } = 5;

        [StepInput("DisableAfterUnload")]
        public bool DisableAfterUnload { get; set; } = false;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new DoubleParameter
            {
                Name = "TargetPosition",
                DisplayName = "目标位置",
                Description = "卸载目标位置（0 表示回零）",
                IsRequired = true,
                DefaultValue = 0,
                Order = 1
            };

            yield return new SelectParameter
            {
                Name = "ControlMode",
                DisplayName = "控制模式",
                Description = "卸载时的控制模式",
                IsRequired = true,
                DefaultValue = "Displacement",
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
                DisplayName = "卸载速率",
                Description = "卸载速率（kN/s 或 mm/s）",
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
                DefaultValue = 5,
                MinValue = 0,
                MaxValue = 60,
                Order = 4
            };

            yield return new BooleanParameter
            {
                Name = "DisableAfterUnload",
                DisplayName = "卸载后禁用",
                Description = "卸载完成后是否禁用控制器",
                DefaultValue = false,
                Order = 5
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var targetPosition = GetParameter<double>("TargetPosition");
            var controlModeStr = GetParameter<string>("ControlMode") ?? "Displacement";
            var rate = GetParameter<double>("Rate");
            var holdSeconds = GetParameter<int>("HoldSeconds");
            var disableAfterUnload = GetParameter<bool>("DisableAfterUnload");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var controlMode = controlModeStr == "Force" ? ControlMode.Force : ControlMode.Displacement;

            testRunContext.AddEvent("UnloadToSafeStep",
                $"开始卸载: 目标 {targetPosition}, 模式 {controlMode}, 速率 {rate}");

            // TODO: 向站点服务发送卸载指令
            // var unloadCommand = new UnloadCommand
            // {
            //     TargetPosition = targetPosition,
            //     ControlMode = controlMode,
            //     Rate = rate
            // };
            // var result = await _stationService.UnloadAsync(unloadCommand, context.CancellationToken);

            // 模拟卸载过程
            var currentValue = controlMode == ControlMode.Displacement
                ? testRunContext.Measurement.Displacement
                : testRunContext.Measurement.Force;
            var unloadTime = Math.Abs(currentValue - targetPosition) / rate;

            await Task.Delay(TimeSpan.FromSeconds(Math.Min(unloadTime, 3)), context.CancellationToken);

            // 更新测量值
            if (controlMode == ControlMode.Displacement)
            {
                testRunContext.Measurement.Displacement = targetPosition;
            }
            else
            {
                testRunContext.Measurement.Force = targetPosition;
            }

            // 保持
            if (holdSeconds > 0)
            {
                testRunContext.AddEvent("UnloadToSafeStep", $"卸载到位，保持 {holdSeconds} 秒");
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(holdSeconds, 3)), context.CancellationToken);
            }

            // 计算残余值
            var residualForce = testRunContext.Measurement.Force;
            var residualDisp = testRunContext.Measurement.Displacement;

            // 禁用控制器（如果需要）
            if (disableAfterUnload)
            {
                // TODO: await _stationService.DisableControllerAsync();
                testRunContext.Machine.IsEnabled = false;
                testRunContext.Machine.CurrentMode = ControlMode.Idle;
                testRunContext.AddEvent("UnloadToSafeStep", "控制器已禁用");
            }

            testRunContext.AddEvent("UnloadToSafeStep",
                $"卸载完成, 残余力: {residualForce:F3} kN, 残余位移: {residualDisp:F3} mm");

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["TargetReached"] = true,
                ["ResidualForce"] = residualForce,
                ["ResidualDisplacement"] = residualDisp,
                ["ControllerDisabled"] = disableAfterUnload
            });
        }
    }
}
