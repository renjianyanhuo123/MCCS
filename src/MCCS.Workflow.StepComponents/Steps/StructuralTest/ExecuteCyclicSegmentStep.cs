using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 循环段执行步骤（正弦/三角/自定义点列）
    /// 入参：幅值、均值、频率/周期、循环数、波形、控制模式
    /// 出参：循环计数、每 N 周期摘要（峰谷、刚度估计）
    /// </summary>
    [StepComponent("execute-cyclic-segment", "执行循环段",
        Description = "执行循环加载段（拟静力/疲劳）",
        Category = ComponentCategory.ControlExecution,
        Icon = "Sine",
        Tags = ["循环", "疲劳", "拟静力", "正弦", "三角波"])]
    public class ExecuteCyclicSegmentStep : BaseWorkflowStep
    {
        [StepInput("Waveform")]
        public string Waveform { get; set; } = "Sine";

        [StepInput("Mean")]
        public double Mean { get; set; } = 0;

        [StepInput("Amplitude")]
        public double Amplitude { get; set; } = 10;

        [StepInput("Frequency")]
        public double Frequency { get; set; } = 1.0;

        [StepInput("Cycles")]
        public int Cycles { get; set; } = 10;

        [StepInput("ControlMode")]
        public string ControlModeStr { get; set; } = "Displacement";

        [StepInput("EveryNCyclesWindow")]
        public int EveryNCyclesWindow { get; set; } = 100;

        [StepInput("AmplitudeSequence")]
        public string AmplitudeSequence { get; set; } = string.Empty;

        [StepInput("CyclesPerLevel")]
        public int CyclesPerLevel { get; set; } = 3;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new SelectParameter
            {
                Name = "Waveform",
                DisplayName = "波形",
                Description = "循环波形类型",
                IsRequired = true,
                DefaultValue = "Sine",
                Options =
                [
                    new SelectOption("Sine", "正弦波"),
                    new SelectOption("Triangle", "三角波"),
                    new SelectOption("Square", "方波")
                ],
                Order = 1
            };

            yield return new DoubleParameter
            {
                Name = "Mean",
                DisplayName = "均值",
                Description = "循环的均值（中心值）",
                IsRequired = true,
                DefaultValue = 0,
                Order = 2
            };

            yield return new DoubleParameter
            {
                Name = "Amplitude",
                DisplayName = "幅值",
                Description = "循环的幅值（半幅）",
                IsRequired = true,
                DefaultValue = 10,
                MinValue = 0,
                Order = 3
            };

            yield return new DoubleParameter
            {
                Name = "Frequency",
                DisplayName = "频率(Hz)",
                Description = "循环频率（拟静力通常 < 0.1Hz，疲劳通常 1-10Hz）",
                IsRequired = true,
                DefaultValue = 1.0,
                MinValue = 0.001,
                MaxValue = 100,
                Order = 4
            };

            yield return new IntegerParameter
            {
                Name = "Cycles",
                DisplayName = "循环数",
                Description = "目标循环次数",
                IsRequired = true,
                DefaultValue = 10,
                MinValue = 1,
                Order = 5
            };

            yield return new SelectParameter
            {
                Name = "ControlMode",
                DisplayName = "控制模式",
                Description = "循环控制模式",
                IsRequired = true,
                DefaultValue = "Displacement",
                Options =
                [
                    new SelectOption("Force", "力控制"),
                    new SelectOption("Displacement", "位移控制")
                ],
                Order = 6
            };

            yield return new IntegerParameter
            {
                Name = "EveryNCyclesWindow",
                DisplayName = "摘要周期间隔",
                Description = "每隔多少个周期输出一次摘要（用于长时间疲劳）",
                IsRequired = true,
                DefaultValue = 100,
                MinValue = 1,
                Order = 7
            };

            yield return new StringParameter
            {
                Name = "AmplitudeSequence",
                DisplayName = "幅值序列",
                Description = "拟静力分级幅值，逗号分隔，如: 1,2,5,10,20（留空则使用固定幅值）",
                IsRequired = false,
                Placeholder = "1,2,5,10,20",
                Order = 8
            };

            yield return new IntegerParameter
            {
                Name = "CyclesPerLevel",
                DisplayName = "每级循环数",
                Description = "拟静力每个幅值级别的循环数",
                IsRequired = true,
                DefaultValue = 3,
                MinValue = 1,
                Order = 9
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var waveform = GetParameter<string>("Waveform") ?? "Sine";
            var mean = GetParameter<double>("Mean");
            var amplitude = GetParameter<double>("Amplitude");
            var frequency = GetParameter<double>("Frequency");
            var cycles = GetParameter<int>("Cycles");
            var controlModeStr = GetParameter<string>("ControlMode") ?? "Displacement";
            var everyNCyclesWindow = GetParameter<int>("EveryNCyclesWindow");
            var amplitudeSequenceStr = GetParameter<string>("AmplitudeSequence");
            var cyclesPerLevel = GetParameter<int>("CyclesPerLevel");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var controlMode = controlModeStr == "Force" ? ControlMode.Force : ControlMode.Displacement;

            var result = new CyclicExecutionResult
            {
                Success = true,
                CycleSummaries = new List<CycleSummary>()
            };

            // 解析幅值序列（拟静力分级）
            var amplitudeSequence = new List<double>();
            if (!string.IsNullOrEmpty(amplitudeSequenceStr))
            {
                amplitudeSequence = amplitudeSequenceStr.Split(',')
                    .Select(s => double.TryParse(s.Trim(), out var v) ? v : 0)
                    .Where(v => v > 0)
                    .ToList();
            }

            // TODO: 向站点服务发送循环执行指令
            // var cyclicCommand = new CyclicCommand
            // {
            //     Waveform = waveform,
            //     Mean = mean,
            //     Amplitude = amplitude,
            //     Frequency = frequency,
            //     Cycles = cycles,
            //     ControlMode = controlMode,
            //     AmplitudeSequence = amplitudeSequence,
            //     CyclesPerLevel = cyclesPerLevel
            // };
            //
            // var execResult = await _stationService.ExecuteCyclicAsync(cyclicCommand,
            //     onCycleSummary: summary => result.CycleSummaries.Add(summary),
            //     cancellationToken: context.CancellationToken);

            // 执行循环
            if (amplitudeSequence.Count > 0)
            {
                // 拟静力分级加载
                testRunContext.AddEvent("ExecuteCyclicSegmentStep",
                    $"开始拟静力分级加载: {amplitudeSequence.Count} 级, 每级 {cyclesPerLevel} 循环");

                foreach (var amp in amplitudeSequence)
                {
                    for (int c = 0; c < cyclesPerLevel; c++)
                    {
                        // 模拟一个循环
                        await Task.Delay(TimeSpan.FromMilliseconds(100), context.CancellationToken);

                        result.CompletedCycles++;
                        testRunContext.CurrentCycleCount = result.CompletedCycles;

                        // 每级第一个循环记录摘要
                        if (c == 0 || result.CompletedCycles % everyNCyclesWindow == 0)
                        {
                            result.CycleSummaries.Add(new CycleSummary
                            {
                                CycleNumber = result.CompletedCycles,
                                PeakForce = controlMode == ControlMode.Force ? mean + amp : (mean + amp) * 0.1,
                                ValleyForce = controlMode == ControlMode.Force ? mean - amp : (mean - amp) * 0.1,
                                PeakDisplacement = controlMode == ControlMode.Displacement ? mean + amp : (mean + amp) * 10,
                                ValleyDisplacement = controlMode == ControlMode.Displacement ? mean - amp : (mean - amp) * 10,
                                EquivalentStiffness = 100.0 / (1 + result.CompletedCycles * 0.0001) // 模拟刚度退化
                            });
                        }
                    }
                    testRunContext.AddEvent("ExecuteCyclicSegmentStep", $"完成幅值级 ±{amp}");
                }
            }
            else
            {
                // 固定幅值循环（疲劳）
                testRunContext.AddEvent("ExecuteCyclicSegmentStep",
                    $"开始循环加载: {cycles} 循环, 频率 {frequency} Hz, 波形 {waveform}");

                var totalTime = cycles / frequency;
                var simulatedCycles = Math.Min(cycles, 100); // 模拟最多 100 个循环

                for (int c = 0; c < simulatedCycles; c++)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), context.CancellationToken);

                    result.CompletedCycles++;
                    testRunContext.CurrentCycleCount = result.CompletedCycles;

                    if (result.CompletedCycles % everyNCyclesWindow == 0 || c == simulatedCycles - 1)
                    {
                        result.CycleSummaries.Add(new CycleSummary
                        {
                            CycleNumber = result.CompletedCycles,
                            PeakForce = controlMode == ControlMode.Force ? mean + amplitude : (mean + amplitude) * 0.1,
                            ValleyForce = controlMode == ControlMode.Force ? mean - amplitude : (mean - amplitude) * 0.1,
                            PeakDisplacement = controlMode == ControlMode.Displacement ? mean + amplitude : (mean + amplitude) * 10,
                            ValleyDisplacement = controlMode == ControlMode.Displacement ? mean - amplitude : (mean - amplitude) * 10,
                            EquivalentStiffness = 100.0 / (1 + result.CompletedCycles * 0.0001)
                        });
                    }
                }

                // 如果目标循环数更多，模拟直接完成
                if (cycles > simulatedCycles)
                {
                    result.CompletedCycles = cycles;
                    testRunContext.CurrentCycleCount = cycles;
                }
            }

            testRunContext.AddEvent("ExecuteCyclicSegmentStep",
                $"循环段完成, 总循环数: {result.CompletedCycles}");

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = result.StoppedByCriteria ? StepResultCode.Warning : StepResultCode.OK,
                ["CompletedCycles"] = result.CompletedCycles,
                ["SummaryCount"] = result.CycleSummaries.Count,
                ["StoppedByCriteria"] = result.StoppedByCriteria,
                ["StopReason"] = result.StopReason,
                ["LastPeakForce"] = result.CycleSummaries.LastOrDefault()?.PeakForce,
                ["LastStiffness"] = result.CycleSummaries.LastOrDefault()?.EquivalentStiffness
            });
        }
    }
}
