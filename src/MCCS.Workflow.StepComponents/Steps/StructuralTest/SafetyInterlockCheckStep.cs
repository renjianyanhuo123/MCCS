using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 安全联锁检查步骤
    /// 入参：Limits、联锁项开关
    /// 出参：InterlockStatus（含失败原因列表）；失败则 Abort
    /// </summary>
    [StepComponent("safety-interlock-check", "安全联锁检查",
        Description = "检查急停、限位、油压、门禁、超载阈值、通道健康等安全联锁",
        Category = ComponentCategory.SafetyAndSetup,
        Icon = "ShieldCheckOutline",
        Tags = ["安全", "联锁", "检查", "急停", "限位"])]
    public class SafetyInterlockCheckStep : BaseWorkflowStep
    {
        [StepInput("CheckEmergencyStop")]
        public bool CheckEmergencyStop { get; set; } = true;

        [StepInput("CheckLimitSwitch")]
        public bool CheckLimitSwitch { get; set; } = true;

        [StepInput("CheckOilPressure")]
        public bool CheckOilPressure { get; set; } = true;

        [StepInput("CheckChannelHealth")]
        public bool CheckChannelHealth { get; set; } = true;

        [StepInput("AbortOnFailure")]
        public bool AbortOnFailure { get; set; } = true;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new BooleanParameter
            {
                Name = "CheckEmergencyStop",
                DisplayName = "检查急停",
                Description = "是否检查急停按钮状态",
                DefaultValue = true,
                Order = 1
            };

            yield return new BooleanParameter
            {
                Name = "CheckLimitSwitch",
                DisplayName = "检查限位",
                Description = "是否检查限位开关状态",
                DefaultValue = true,
                Order = 2
            };

            yield return new BooleanParameter
            {
                Name = "CheckOilPressure",
                DisplayName = "检查油压",
                Description = "是否检查液压站油压",
                DefaultValue = true,
                Order = 3
            };

            yield return new BooleanParameter
            {
                Name = "CheckChannelHealth",
                DisplayName = "检查通道健康",
                Description = "是否检查测量通道健康状态",
                DefaultValue = true,
                Order = 4
            };

            yield return new BooleanParameter
            {
                Name = "AbortOnFailure",
                DisplayName = "失败时终止",
                Description = "联锁检查失败时是否终止工作流",
                DefaultValue = true,
                Order = 5
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var checkEmergencyStop = GetParameter<bool>("CheckEmergencyStop");
            var checkLimitSwitch = GetParameter<bool>("CheckLimitSwitch");
            var checkOilPressure = GetParameter<bool>("CheckOilPressure");
            var checkChannelHealth = GetParameter<bool>("CheckChannelHealth");
            var abortOnFailure = GetParameter<bool>("AbortOnFailure");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var limits = testRunContext.Recipe.Limits;
            var interlockStatus = new InterlockStatus { AllPassed = true };

            // TODO: 向站点服务发送联锁检查指令

            // 急停检查
            if (checkEmergencyStop)
            {
                // TODO: var estopStatus = await _stationService.GetEmergencyStopStatusAsync();
                var estopPressed = false; // 模拟：急停未按下
                var item = new InterlockCheckItem
                {
                    Name = "急停按钮",
                    Passed = !estopPressed,
                    CurrentValue = estopPressed ? "按下" : "正常",
                    ExpectedValue = "正常"
                };
                if (estopPressed)
                {
                    item.FailureReason = "急停按钮已按下";
                    interlockStatus.FailureReasons.Add("急停按钮已按下");
                    interlockStatus.AllPassed = false;
                }
                interlockStatus.Items.Add(item);
            }

            // 限位检查
            if (checkLimitSwitch)
            {
                // TODO: var limitStatus = await _stationService.GetLimitSwitchStatusAsync();
                var limitTriggered = false; // 模拟：限位未触发
                var item = new InterlockCheckItem
                {
                    Name = "限位开关",
                    Passed = !limitTriggered,
                    CurrentValue = limitTriggered ? "触发" : "正常",
                    ExpectedValue = "正常"
                };
                if (limitTriggered)
                {
                    item.FailureReason = "限位开关已触发";
                    interlockStatus.FailureReasons.Add("限位开关已触发");
                    interlockStatus.AllPassed = false;
                }
                interlockStatus.Items.Add(item);
            }

            // 油压检查
            if (checkOilPressure)
            {
                // TODO: var pressure = await _stationService.GetOilPressureAsync();
                var pressure = 21.0; // 模拟油压值
                var passed = pressure >= limits.MinOilPressure && pressure <= limits.MaxOilPressure;
                var item = new InterlockCheckItem
                {
                    Name = "液压油压",
                    Passed = passed,
                    CurrentValue = $"{pressure:F1} MPa",
                    ExpectedValue = $"{limits.MinOilPressure:F1} ~ {limits.MaxOilPressure:F1} MPa"
                };
                if (!passed)
                {
                    item.FailureReason = pressure < limits.MinOilPressure ? "油压过低" : "油压过高";
                    interlockStatus.FailureReasons.Add(item.FailureReason);
                    interlockStatus.AllPassed = false;
                }
                interlockStatus.Items.Add(item);
                testRunContext.Machine.OilPressure = pressure;
            }

            // 通道健康检查
            if (checkChannelHealth)
            {
                // TODO: var channelStatus = await _stationService.GetChannelHealthStatusAsync();
                var allChannelsHealthy = true; // 模拟：所有通道健康
                var item = new InterlockCheckItem
                {
                    Name = "测量通道",
                    Passed = allChannelsHealthy,
                    CurrentValue = allChannelsHealthy ? "全部正常" : "存在异常",
                    ExpectedValue = "全部正常"
                };
                if (!allChannelsHealthy)
                {
                    item.FailureReason = "存在异常测量通道";
                    interlockStatus.FailureReasons.Add("存在异常测量通道");
                    interlockStatus.AllPassed = false;
                }
                interlockStatus.Items.Add(item);
            }

            // 记录事件
            if (interlockStatus.AllPassed)
            {
                testRunContext.AddEvent("SafetyInterlockCheckStep", "安全联锁检查通过");
            }
            else
            {
                testRunContext.AddEvent("SafetyInterlockCheckStep",
                    $"安全联锁检查失败: {string.Join("; ", interlockStatus.FailureReasons)}",
                    EventLevel.Warning);
            }

            context.SetVariable("InterlockStatus", interlockStatus);

            await Task.CompletedTask;

            if (!interlockStatus.AllPassed && abortOnFailure)
            {
                return StepResult.Fail($"安全联锁检查失败: {string.Join("; ", interlockStatus.FailureReasons)}");
            }

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = interlockStatus.AllPassed ? StepResultCode.OK : StepResultCode.Warning,
                ["AllPassed"] = interlockStatus.AllPassed,
                ["FailureReasons"] = interlockStatus.FailureReasons,
                ["CheckItems"] = interlockStatus.Items.Select(i => new { i.Name, i.Passed, i.CurrentValue }).ToList()
            });
        }
    }
}
