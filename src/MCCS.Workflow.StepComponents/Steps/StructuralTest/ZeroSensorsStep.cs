using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 传感器清零/基线采集步骤
    /// 入参：要清零的通道列表、稳定时间、漂移阈值
    /// 出参：ZeroOffsets、漂移评估结果
    /// </summary>
    [StepComponent("zero-sensors", "传感器清零",
        Description = "通道清零/基线采集（零点、漂移评估）",
        Category = ComponentCategory.ManualOperation,
        Icon = "NumericZeroCircleOutline",
        Tags = ["清零", "基线", "传感器", "零点"])]
    public class ZeroSensorsStep : BaseWorkflowStep
    {
        [StepInput("Channels")]
        public string Channels { get; set; } = string.Empty;

        [StepInput("StabilizeSeconds")]
        public int StabilizeSeconds { get; set; } = 5;

        [StepInput("DriftThreshold")]
        public double DriftThreshold { get; set; } = 0.1;

        [StepInput("SampleCount")]
        public int SampleCount { get; set; } = 100;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "Channels",
                DisplayName = "通道列表",
                Description = "要清零的通道ID，逗号分隔，留空表示所有通道",
                IsRequired = false,
                Placeholder = "Force,Displacement,Strain",
                Order = 1
            };

            yield return new IntegerParameter
            {
                Name = "StabilizeSeconds",
                DisplayName = "稳定时间(秒)",
                Description = "清零前等待系统稳定的时间",
                IsRequired = true,
                DefaultValue = 5,
                MinValue = 1,
                MaxValue = 60,
                Order = 2
            };

            yield return new DoubleParameter
            {
                Name = "DriftThreshold",
                DisplayName = "漂移阈值(%)",
                Description = "允许的最大漂移百分比",
                IsRequired = true,
                DefaultValue = 0.1,
                MinValue = 0.01,
                MaxValue = 1.0,
                Order = 3
            };

            yield return new IntegerParameter
            {
                Name = "SampleCount",
                DisplayName = "采样点数",
                Description = "用于评估漂移的采样点数",
                IsRequired = true,
                DefaultValue = 100,
                MinValue = 10,
                MaxValue = 1000,
                Order = 4
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var channelsStr = GetParameter<string>("Channels");
            var stabilizeSeconds = GetParameter<int>("StabilizeSeconds");
            var driftThreshold = GetParameter<double>("DriftThreshold");
            var sampleCount = GetParameter<int>("SampleCount");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            // 解析通道列表
            List<string> channels;
            if (string.IsNullOrEmpty(channelsStr))
            {
                // 使用配方中定义的所有通道
                channels = new List<string> { "Force", "Displacement", "Strain" };
            }
            else
            {
                channels = channelsStr.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            // 等待系统稳定
            testRunContext.AddEvent("ZeroSensorsStep", $"等待系统稳定 {stabilizeSeconds} 秒...");
            await Task.Delay(TimeSpan.FromSeconds(stabilizeSeconds), context.CancellationToken);

            var zeroResults = new List<ZeroOffsetResult>();
            var allWithinThreshold = true;

            // TODO: 向站点服务发送清零指令
            foreach (var channel in channels)
            {
                // TODO: var result = await _stationService.ZeroChannelAsync(channel, sampleCount);
                // 模拟清零结果
                var offset = (new Random().NextDouble() - 0.5) * 0.01;
                var drift = new Random().NextDouble() * driftThreshold * 0.5;
                var withinThreshold = drift <= driftThreshold;

                var result = new ZeroOffsetResult
                {
                    ChannelId = channel,
                    ChannelName = channel,
                    Offset = offset,
                    Drift = drift,
                    WithinThreshold = withinThreshold
                };

                zeroResults.Add(result);

                if (!withinThreshold)
                {
                    allWithinThreshold = false;
                    testRunContext.AddEvent("ZeroSensorsStep",
                        $"通道 {channel} 漂移超限: {drift:F3}% > {driftThreshold:F3}%",
                        EventLevel.Warning);
                }
            }

            // 记录到上下文
            testRunContext.SetStepOutput("ZeroOffsets", zeroResults);
            testRunContext.AddEvent("ZeroSensorsStep",
                $"传感器清零完成, {channels.Count} 个通道, 全部在阈值内: {allWithinThreshold}");

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = allWithinThreshold ? StepResultCode.OK : StepResultCode.Warning,
                ["Channels"] = channels,
                ["ZeroOffsets"] = zeroResults.Select(r => new { r.ChannelId, r.Offset, r.Drift, r.WithinThreshold }).ToList(),
                ["AllWithinThreshold"] = allWithinThreshold
            });
        }
    }
}
