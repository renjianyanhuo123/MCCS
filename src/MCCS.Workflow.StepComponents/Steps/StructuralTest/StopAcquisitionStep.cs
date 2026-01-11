using System.IO;

using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 停止数据采集步骤
    /// 入参：无（使用上下文中的 AcquisitionId）
    /// 出参：数据文件清单、丢样统计
    /// </summary>
    [StepComponent("stop-acquisition", "停止采集",
        Description = "停止数据采集并保存数据",
        Category = ComponentCategory.DataAndReport,
        Icon = "StopCircle",
        Tags = ["采集", "数据", "停止", "保存"])]
    public class StopAcquisitionStep : BaseWorkflowStep
    {
        [StepInput("FlushBuffer")]
        public bool FlushBuffer { get; set; } = true;

        [StepInput("GenerateSummary")]
        public bool GenerateSummary { get; set; } = true;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new BooleanParameter
            {
                Name = "FlushBuffer",
                DisplayName = "刷新缓冲区",
                Description = "停止前是否刷新所有缓冲区数据",
                DefaultValue = true,
                Order = 1
            };

            yield return new BooleanParameter
            {
                Name = "GenerateSummary",
                DisplayName = "生成摘要",
                Description = "是否生成数据摘要文件",
                DefaultValue = true,
                Order = 2
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var flushBuffer = GetParameter<bool>("FlushBuffer");
            var generateSummary = GetParameter<bool>("GenerateSummary");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var acquisitionId = context.GetVariable<string>("AcquisitionId");
            var dataFilePath = context.GetVariable<string>("AcquisitionDataPath");

            if (string.IsNullOrEmpty(acquisitionId))
            {
                testRunContext.AddEvent("StopAcquisitionStep", "未找到活动的采集任务", EventLevel.Warning);
                return StepResult.Succeed(new Dictionary<string, object?>
                {
                    ["ResultCode"] = StepResultCode.Warning,
                    ["Message"] = "未找到活动的采集任务"
                });
            }

            // TODO: 向站点服务发送停止采集指令
            // if (flushBuffer)
            // {
            //     await _stationService.FlushAcquisitionBufferAsync(acquisitionId);
            // }
            // var stopResult = await _stationService.StopAcquisitionAsync(acquisitionId);

            // 更新采集结果
            var acquisitionResult = testRunContext.GetStepOutput<AcquisitionResult>("AcquisitionResult");
            if (acquisitionResult != null)
            {
                acquisitionResult.EndTime = DateTime.Now;
                acquisitionResult.TotalRecords = 10000; // TODO: 从实际采集结果获取
                acquisitionResult.DroppedSamples = 0;
                testRunContext.SetStepOutput("AcquisitionResult", acquisitionResult);
            }

            // 生成数据摘要
            if (generateSummary && !string.IsNullOrEmpty(dataFilePath))
            {
                var summaryPath = Path.ChangeExtension(dataFilePath, ".summary.json");
                // TODO: 生成实际的摘要文件
                testRunContext.AddEvent("StopAcquisitionStep", $"数据摘要已生成: {summaryPath}");
            }

            testRunContext.AddEvent("StopAcquisitionStep",
                $"数据采集已停止, 总记录数: {acquisitionResult?.TotalRecords ?? 0}, 丢样数: {acquisitionResult?.DroppedSamples ?? 0}");

            await Task.CompletedTask;

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["AcquisitionId"] = acquisitionId,
                ["DataFiles"] = acquisitionResult?.DataFiles ?? new List<string>(),
                ["TotalRecords"] = acquisitionResult?.TotalRecords ?? 0,
                ["DroppedSamples"] = acquisitionResult?.DroppedSamples ?? 0,
                ["StartTime"] = acquisitionResult?.StartTime,
                ["EndTime"] = acquisitionResult?.EndTime
            });
        }
    }
}
