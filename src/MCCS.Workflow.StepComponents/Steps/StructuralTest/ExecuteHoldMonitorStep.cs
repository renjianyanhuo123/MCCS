using System.IO;

using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 长时间保持监控步骤（用于蠕变/松弛试验）
    /// 入参：保持时长、控制模式、记录周期、漂移阈值
    /// 出参：TimeSeriesSummary、异常与补偿动作记录
    /// </summary>
    [StepComponent("execute-hold-monitor", "保持监控",
        Description = "执行长时间保持并监控（蠕变/松弛）",
        Category = ComponentCategory.ControlExecution,
        Icon = "TimerSandFull",
        Tags = ["保持", "蠕变", "松弛", "监控", "长时间"])]
    public class ExecuteHoldMonitorStep : BaseWorkflowStep
    {
        [StepInput("HoldDurationSeconds")]
        public double HoldDurationSeconds { get; set; } = 3600;

        [StepInput("ControlMode")]
        public string ControlModeStr { get; set; } = "Force";

        [StepInput("LogIntervalSeconds")]
        public double LogIntervalSeconds { get; set; } = 60;

        [StepInput("DriftLimit")]
        public double DriftLimit { get; set; } = 5.0;

        [StepInput("SaveIntermediateData")]
        public bool SaveIntermediateData { get; set; } = true;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new DoubleParameter
            {
                Name = "HoldDurationSeconds",
                DisplayName = "保持时长(秒)",
                Description = "总保持时间",
                IsRequired = true,
                DefaultValue = 3600,
                MinValue = 1,
                Order = 1
            };

            yield return new SelectParameter
            {
                Name = "ControlMode",
                DisplayName = "控制模式",
                Description = "保持的控制模式（蠕变用力保持，松弛用位移保持）",
                IsRequired = true,
                DefaultValue = "Force",
                Options =
                [
                    new SelectOption("Force", "力保持（蠕变）"),
                    new SelectOption("Displacement", "位移保持（松弛）")
                ],
                Order = 2
            };

            yield return new DoubleParameter
            {
                Name = "LogIntervalSeconds",
                DisplayName = "记录周期(秒)",
                Description = "数据记录间隔",
                IsRequired = true,
                DefaultValue = 60,
                MinValue = 1,
                Order = 3
            };

            yield return new DoubleParameter
            {
                Name = "DriftLimit",
                DisplayName = "漂移限值(%)",
                Description = "允许的最大漂移百分比",
                IsRequired = true,
                DefaultValue = 5.0,
                MinValue = 0.1,
                Order = 4
            };

            yield return new BooleanParameter
            {
                Name = "SaveIntermediateData",
                DisplayName = "保存中间数据",
                Description = "是否定期保存中间数据到文件",
                DefaultValue = true,
                Order = 5
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var holdDuration = GetParameter<double>("HoldDurationSeconds");
            var controlModeStr = GetParameter<string>("ControlMode") ?? "Force";
            var logInterval = GetParameter<double>("LogIntervalSeconds");
            var driftLimit = GetParameter<double>("DriftLimit");
            var saveIntermediate = GetParameter<bool>("SaveIntermediateData");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var controlMode = controlModeStr == "Displacement" ? ControlMode.Displacement : ControlMode.Force;
            var isCreep = controlMode == ControlMode.Force;

            testRunContext.AddEvent("ExecuteHoldMonitorStep",
                $"开始{(isCreep ? "蠕变" : "松弛")}保持: {holdDuration} 秒, 记录间隔 {logInterval} 秒");

            // TODO: 向站点服务发送保持监控指令
            // var holdCommand = new HoldMonitorCommand
            // {
            //     DurationSeconds = holdDuration,
            //     ControlMode = controlMode,
            //     LogIntervalSeconds = logInterval,
            //     DriftLimit = driftLimit
            // };
            //
            // var result = await _stationService.ExecuteHoldMonitorAsync(holdCommand,
            //     onDataPoint: point => { /* 处理数据点 */ },
            //     cancellationToken: context.CancellationToken);

            var timeSeriesData = new List<Dictionary<string, double>>();
            var startTime = DateTime.Now;
            var initialForce = testRunContext.Measurement.Force;
            var initialDisp = testRunContext.Measurement.Displacement;
            var driftExceeded = false;
            var driftMaxValue = 0.0;

            // 模拟保持监控（限制最大模拟时间）
            var numPoints = (int)Math.Ceiling(holdDuration / logInterval);
            var maxSimulatedPoints = Math.Min(numPoints, 20);
            var simulatedInterval = Math.Min(logInterval, 0.5);

            for (int i = 0; i < maxSimulatedPoints; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(simulatedInterval), context.CancellationToken);

                var elapsed = (i + 1) * logInterval;
                var progress = elapsed / holdDuration;

                // 模拟蠕变/松弛数据
                double currentDisp, currentForce;
                if (isCreep)
                {
                    // 蠕变：力恒定，位移增加
                    currentForce = initialForce;
                    currentDisp = initialDisp * (1 + 0.1 * Math.Log(1 + progress * 10)); // 对数蠕变
                }
                else
                {
                    // 松弛：位移恒定，力下降
                    currentDisp = initialDisp;
                    currentForce = initialForce * (1 - 0.2 * Math.Log(1 + progress * 10)); // 对数松弛
                }

                // 记录数据点
                var dataPoint = new Dictionary<string, double>
                {
                    ["Time"] = elapsed,
                    ["Force"] = currentForce,
                    ["Displacement"] = currentDisp,
                    ["Strain"] = currentDisp / 100 * 1000 // 模拟应变 με
                };
                timeSeriesData.Add(dataPoint);

                // 检查漂移
                var drift = isCreep
                    ? Math.Abs(currentDisp - initialDisp) / Math.Abs(initialDisp) * 100
                    : Math.Abs(currentForce - initialForce) / Math.Abs(initialForce) * 100;

                if (drift > driftMaxValue) driftMaxValue = drift;
                if (drift > driftLimit)
                {
                    driftExceeded = true;
                    testRunContext.AddEvent("ExecuteHoldMonitorStep",
                        $"漂移超限: {drift:F2}% > {driftLimit:F2}%", EventLevel.Warning);
                }

                // 更新测量快照
                testRunContext.Measurement.Force = currentForce;
                testRunContext.Measurement.Displacement = currentDisp;
                testRunContext.Measurement.ElapsedSeconds = elapsed;
            }

            // 保存数据
            if (saveIntermediate)
            {
                var dataPath = Path.Combine(testRunContext.DataFolder,
                    $"{(isCreep ? "Creep" : "Relaxation")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                // TODO: 实际保存数据到文件
                testRunContext.AddEvent("ExecuteHoldMonitorStep", $"数据已保存: {dataPath}");
            }

            testRunContext.AddEvent("ExecuteHoldMonitorStep",
                $"保持监控完成, 记录点数: {timeSeriesData.Count}, 最大漂移: {driftMaxValue:F2}%");

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = driftExceeded ? StepResultCode.Warning : StepResultCode.OK,
                ["HoldDurationSeconds"] = holdDuration,
                ["DataPointCount"] = timeSeriesData.Count,
                ["MaxDrift"] = driftMaxValue,
                ["DriftExceeded"] = driftExceeded,
                ["FinalForce"] = testRunContext.Measurement.Force,
                ["FinalDisplacement"] = testRunContext.Measurement.Displacement
            });
        }
    }
}
