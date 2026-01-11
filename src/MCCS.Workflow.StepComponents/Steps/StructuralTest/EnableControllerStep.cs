using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 控制器使能步骤
    /// 入参：控制模式候选（先 Idle）
    /// 出参：MachineState.Enabled=true
    /// </summary>
    [StepComponent("enable-controller", "控制器使能",
        Description = "使能控制器，设置初始控制模式",
        Category = ComponentCategory.SafetyAndSetup,
        Icon = "PowerOn",
        Tags = ["使能", "控制器", "电源"])]
    public class EnableControllerStep : BaseWorkflowStep
    {
        [StepInput("InitialMode")]
        public string InitialMode { get; set; } = "Idle";

        [StepInput("StartHydraulic")]
        public bool StartHydraulic { get; set; } = true;

        [StepInput("WaitStabilizeSeconds")]
        public int WaitStabilizeSeconds { get; set; } = 5;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new SelectParameter
            {
                Name = "InitialMode",
                DisplayName = "初始控制模式",
                Description = "使能后的初始控制模式",
                IsRequired = true,
                DefaultValue = "Idle",
                Options =
                [
                    new SelectOption("Idle", "空闲"),
                    new SelectOption("Force", "力控制"),
                    new SelectOption("Displacement", "位移控制"),
                    new SelectOption("Strain", "应变控制")
                ],
                Order = 1
            };

            yield return new BooleanParameter
            {
                Name = "StartHydraulic",
                DisplayName = "启动液压站",
                Description = "使能时是否同时启动液压站",
                DefaultValue = true,
                Order = 2
            };

            yield return new IntegerParameter
            {
                Name = "WaitStabilizeSeconds",
                DisplayName = "稳定等待时间(秒)",
                Description = "使能后等待系统稳定的时间",
                IsRequired = true,
                DefaultValue = 5,
                MinValue = 0,
                MaxValue = 60,
                Order = 3
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var initialModeStr = GetParameter<string>("InitialMode") ?? "Idle";
            var startHydraulic = GetParameter<bool>("StartHydraulic");
            var waitSeconds = GetParameter<int>("WaitStabilizeSeconds");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            // 解析控制模式
            var initialMode = initialModeStr switch
            {
                "Force" => ControlMode.Force,
                "Displacement" => ControlMode.Displacement,
                "Strain" => ControlMode.Strain,
                _ => ControlMode.Idle
            };

            // TODO: 向站点服务发送使能指令

            // 启动液压站（如果需要）
            if (startHydraulic)
            {
                // TODO: await _stationService.StartHydraulicAsync();
                testRunContext.Machine.HydraulicRunning = true;
                testRunContext.AddEvent("EnableControllerStep", "液压站启动");
            }

            // 使能控制器
            // TODO: await _stationService.EnableControllerAsync(initialMode);
            testRunContext.Machine.IsEnabled = true;
            testRunContext.Machine.CurrentMode = initialMode;
            testRunContext.AddEvent("EnableControllerStep", $"控制器使能成功，模式: {initialMode}");

            // 等待系统稳定
            if (waitSeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), context.CancellationToken);
            }

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["IsEnabled"] = true,
                ["CurrentMode"] = initialMode.ToString(),
                ["HydraulicRunning"] = startHydraulic
            });
        }
    }
}
