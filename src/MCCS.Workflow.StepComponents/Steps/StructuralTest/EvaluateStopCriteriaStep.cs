using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 停机准则评估步骤
    /// 入参：StopCriteria（破坏判据、次数、时间、刚度衰减、异常通道）
    /// 出参：ShouldStop + Reason
    /// </summary>
    [StepComponent("evaluate-stop-criteria", "停机准则评估",
        Description = "评估是否满足停机准则（破坏、次数、时间、刚度衰减等）",
        Category = ComponentCategory.ControlExecution,
        Icon = "StopCircleOutline",
        Tags = ["停机", "准则", "破坏", "判定"])]
    public class EvaluateStopCriteriaStep : BaseWorkflowStep
    {
        [StepInput("CheckFailure")]
        public bool CheckFailure { get; set; } = true;

        [StepInput("FailureForceDropRatio")]
        public double FailureForceDropRatio { get; set; } = 0.2;

        [StepInput("CheckMaxCycles")]
        public bool CheckMaxCycles { get; set; } = true;

        [StepInput("MaxCycles")]
        public int MaxCycles { get; set; } = 1000000;

        [StepInput("CheckMaxTime")]
        public bool CheckMaxTime { get; set; } = false;

        [StepInput("MaxTimeSeconds")]
        public double MaxTimeSeconds { get; set; } = 86400;

        [StepInput("CheckStiffnessDegradation")]
        public bool CheckStiffnessDegradation { get; set; } = false;

        [StepInput("StiffnessDegradationRatio")]
        public double StiffnessDegradationRatio { get; set; } = 0.5;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new BooleanParameter
            {
                Name = "CheckFailure",
                DisplayName = "检查破坏",
                Description = "是否检查试件破坏（承载力下降）",
                DefaultValue = true,
                Order = 1
            };

            yield return new DoubleParameter
            {
                Name = "FailureForceDropRatio",
                DisplayName = "破坏判定：力下降比例",
                Description = "力下降到峰值的多少比例判定为破坏（如0.2表示下降20%）",
                IsRequired = true,
                DefaultValue = 0.2,
                MinValue = 0.05,
                MaxValue = 0.9,
                Order = 2
            };

            yield return new BooleanParameter
            {
                Name = "CheckMaxCycles",
                DisplayName = "检查最大循环数",
                Description = "是否检查是否达到最大循环数",
                DefaultValue = true,
                Order = 3
            };

            yield return new IntegerParameter
            {
                Name = "MaxCycles",
                DisplayName = "最大循环数",
                Description = "最大循环次数",
                IsRequired = true,
                DefaultValue = 1000000,
                MinValue = 1,
                Order = 4
            };

            yield return new BooleanParameter
            {
                Name = "CheckMaxTime",
                DisplayName = "检查最大时间",
                Description = "是否检查是否达到最大试验时间",
                DefaultValue = false,
                Order = 5
            };

            yield return new DoubleParameter
            {
                Name = "MaxTimeSeconds",
                DisplayName = "最大时间(秒)",
                Description = "最大试验时间",
                IsRequired = true,
                DefaultValue = 86400,
                MinValue = 1,
                Order = 6
            };

            yield return new BooleanParameter
            {
                Name = "CheckStiffnessDegradation",
                DisplayName = "检查刚度衰减",
                Description = "是否检查刚度衰减",
                DefaultValue = false,
                Order = 7
            };

            yield return new DoubleParameter
            {
                Name = "StiffnessDegradationRatio",
                DisplayName = "刚度衰减阈值比例",
                Description = "刚度下降到初始值的多少比例时停止（如0.5表示下降到50%）",
                IsRequired = true,
                DefaultValue = 0.5,
                MinValue = 0.1,
                MaxValue = 0.9,
                Order = 8
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var checkFailure = GetParameter<bool>("CheckFailure");
            var failureDropRatio = GetParameter<double>("FailureForceDropRatio");
            var checkMaxCycles = GetParameter<bool>("CheckMaxCycles");
            var maxCycles = GetParameter<int>("MaxCycles");
            var checkMaxTime = GetParameter<bool>("CheckMaxTime");
            var maxTimeSeconds = GetParameter<double>("MaxTimeSeconds");
            var checkStiffness = GetParameter<bool>("CheckStiffnessDegradation");
            var stiffnessRatio = GetParameter<double>("StiffnessDegradationRatio");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var result = new StopCriteriaResult
            {
                ShouldStop = false,
                Reason = "未触发任何停机准则"
            };

            // TODO: 从站点服务获取实时数据进行判断
            // var currentData = await _stationService.GetCurrentMeasurementAsync();

            var measurement = testRunContext.Measurement;
            var elapsedSeconds = (DateTime.Now - testRunContext.StartTime).TotalSeconds;

            // 检查破坏（承载力下降）
            if (checkFailure)
            {
                var peakForce = measurement.PeakForce;
                var currentForce = Math.Abs(measurement.Force);
                if (peakForce > 0 && currentForce < peakForce * (1 - failureDropRatio))
                {
                    result.ShouldStop = true;
                    result.Reason = $"试件破坏：当前力 {currentForce:F2} kN 下降到峰值 {peakForce:F2} kN 的 {(1 - failureDropRatio) * 100:F0}% 以下";
                    result.TriggeredCriteria = "Failure";
                    result.CurrentValue = currentForce;
                    result.ThresholdValue = peakForce * (1 - failureDropRatio);
                }
            }

            // 检查最大循环数
            if (!result.ShouldStop && checkMaxCycles)
            {
                var currentCycles = testRunContext.CurrentCycleCount;
                if (currentCycles >= maxCycles)
                {
                    result.ShouldStop = true;
                    result.Reason = $"达到最大循环数: {currentCycles} >= {maxCycles}";
                    result.TriggeredCriteria = "MaxCycles";
                    result.CurrentValue = currentCycles;
                    result.ThresholdValue = maxCycles;
                }
            }

            // 检查最大时间
            if (!result.ShouldStop && checkMaxTime)
            {
                if (elapsedSeconds >= maxTimeSeconds)
                {
                    result.ShouldStop = true;
                    result.Reason = $"达到最大试验时间: {elapsedSeconds:F0}s >= {maxTimeSeconds:F0}s";
                    result.TriggeredCriteria = "MaxTime";
                    result.CurrentValue = elapsedSeconds;
                    result.ThresholdValue = maxTimeSeconds;
                }
            }

            // 检查刚度衰减
            if (!result.ShouldStop && checkStiffness)
            {
                // 从循环摘要中获取刚度
                var cyclicResult = testRunContext.GetStepOutput<CyclicExecutionResult>("CyclicResult");
                if (cyclicResult?.CycleSummaries.Count >= 2)
                {
                    var initialStiffness = cyclicResult.CycleSummaries.First().EquivalentStiffness;
                    var currentStiffness = cyclicResult.CycleSummaries.Last().EquivalentStiffness;
                    if (initialStiffness > 0 && currentStiffness < initialStiffness * stiffnessRatio)
                    {
                        result.ShouldStop = true;
                        result.Reason = $"刚度衰减: 当前刚度 {currentStiffness:F2} 下降到初始 {initialStiffness:F2} 的 {stiffnessRatio * 100:F0}% 以下";
                        result.TriggeredCriteria = "StiffnessDegradation";
                        result.CurrentValue = currentStiffness;
                        result.ThresholdValue = initialStiffness * stiffnessRatio;
                    }
                }
            }

            // 记录事件
            if (result.ShouldStop)
            {
                testRunContext.AddEvent("EvaluateStopCriteriaStep", $"触发停机准则: {result.Reason}", EventLevel.Warning);
            }
            else
            {
                testRunContext.AddEvent("EvaluateStopCriteriaStep", "停机准则检查通过，继续试验");
            }

            context.SetVariable("StopCriteriaResult", result);

            await Task.CompletedTask;

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = result.ShouldStop ? StepResultCode.Warning : StepResultCode.OK,
                ["ShouldStop"] = result.ShouldStop,
                ["Reason"] = result.Reason,
                ["TriggeredCriteria"] = result.TriggeredCriteria,
                ["CurrentValue"] = result.CurrentValue,
                ["ThresholdValue"] = result.ThresholdValue
            });
        }
    }
}
