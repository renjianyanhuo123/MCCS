using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;
using System.Text.Json;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 报告生成步骤
    /// 入参：模板、报告字段映射
    /// 出参：报告路径、摘要指标
    /// </summary>
    [StepComponent("generate-report", "生成报告",
        Description = "生成试验报告（数据、校准信息、事件日志）",
        Category = ComponentCategory.DataAndReport,
        Icon = "FileDocumentEditOutline",
        Tags = ["报告", "生成", "导出", "文档"])]
    public class GenerateReportStep : BaseWorkflowStep
    {
        [StepInput("ReportTemplate")]
        public string ReportTemplate { get; set; } = "Standard";

        [StepInput("ReportFormat")]
        public string ReportFormat { get; set; } = "JSON";

        [StepInput("IncludeRawData")]
        public bool IncludeRawData { get; set; } = false;

        [StepInput("IncludeEventLog")]
        public bool IncludeEventLog { get; set; } = true;

        [StepInput("IncludeCalibrationInfo")]
        public bool IncludeCalibrationInfo { get; set; } = true;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new SelectParameter
            {
                Name = "ReportTemplate",
                DisplayName = "报告模板",
                Description = "使用的报告模板",
                IsRequired = true,
                DefaultValue = "Standard",
                Options =
                [
                    new SelectOption("Standard", "标准报告"),
                    new SelectOption("Detailed", "详细报告"),
                    new SelectOption("Summary", "摘要报告"),
                    new SelectOption("Custom", "自定义模板")
                ],
                Order = 1
            };

            yield return new SelectParameter
            {
                Name = "ReportFormat",
                DisplayName = "报告格式",
                Description = "报告输出格式",
                IsRequired = true,
                DefaultValue = "JSON",
                Options =
                [
                    new SelectOption("JSON", "JSON"),
                    new SelectOption("PDF", "PDF"),
                    new SelectOption("Word", "Word"),
                    new SelectOption("Excel", "Excel"),
                    new SelectOption("HTML", "HTML")
                ],
                Order = 2
            };

            yield return new BooleanParameter
            {
                Name = "IncludeRawData",
                DisplayName = "包含原始数据",
                Description = "是否在报告中包含原始测量数据",
                DefaultValue = false,
                Order = 3
            };

            yield return new BooleanParameter
            {
                Name = "IncludeEventLog",
                DisplayName = "包含事件日志",
                Description = "是否在报告中包含事件日志",
                DefaultValue = true,
                Order = 4
            };

            yield return new BooleanParameter
            {
                Name = "IncludeCalibrationInfo",
                DisplayName = "包含校准信息",
                Description = "是否在报告中包含校准/核查信息",
                DefaultValue = true,
                Order = 5
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var template = GetParameter<string>("ReportTemplate") ?? "Standard";
            var format = GetParameter<string>("ReportFormat") ?? "JSON";
            var includeRawData = GetParameter<bool>("IncludeRawData");
            var includeEventLog = GetParameter<bool>("IncludeEventLog");
            var includeCalibrationInfo = GetParameter<bool>("IncludeCalibrationInfo");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            // 计算报告时间
            testRunContext.EndTime = DateTime.Now;

            // 生成报告文件路径
            var extension = format.ToLower() switch
            {
                "pdf" => ".pdf",
                "word" => ".docx",
                "excel" => ".xlsx",
                "html" => ".html",
                _ => ".json"
            };
            var reportPath = Path.Combine(testRunContext.DataFolder,
                $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}{extension}");

            // 收集摘要指标
            var summaryMetrics = new Dictionary<string, object?>
            {
                ["RunId"] = testRunContext.RunId.ToString(),
                ["TestType"] = testRunContext.Recipe.TestType.ToString(),
                ["RecipeName"] = testRunContext.Recipe.Name,
                ["Operator"] = testRunContext.Operator,
                ["SpecimenId"] = testRunContext.SpecimenId,
                ["StartTime"] = testRunContext.StartTime,
                ["EndTime"] = testRunContext.EndTime,
                ["Duration"] = (testRunContext.EndTime - testRunContext.StartTime)?.TotalSeconds,
                ["TotalCycles"] = testRunContext.CurrentCycleCount,
                ["PeakForce"] = testRunContext.Measurement.PeakForce,
                ["PeakDisplacement"] = testRunContext.Measurement.PeakDisplacement
            };

            // 构建报告内容
            var reportContent = new Dictionary<string, object?>
            {
                ["Header"] = new
                {
                    ReportId = Guid.NewGuid().ToString(),
                    GeneratedAt = DateTime.Now,
                    Template = template,
                    Format = format
                },
                ["TestInfo"] = new
                {
                    testRunContext.RunId,
                    Recipe = testRunContext.Recipe.Name,
                    TestType = testRunContext.Recipe.TestType.ToString(),
                    ControlMode = testRunContext.Recipe.ControlMode.ToString(),
                    testRunContext.Operator,
                    testRunContext.SpecimenId,
                    testRunContext.StartTime,
                    testRunContext.EndTime
                },
                ["Summary"] = summaryMetrics
            };

            // 添加校准信息
            if (includeCalibrationInfo)
            {
                var forceVerification = testRunContext.GetStepOutput<VerificationReport>("ForceVerification");
                var extVerification = testRunContext.GetStepOutput<VerificationReport>("ExtensometerVerification");
                reportContent["Calibration"] = new
                {
                    ForceVerification = forceVerification != null ? new
                    {
                        forceVerification.ReportId,
                        forceVerification.CalibrationDate,
                        forceVerification.Passed,
                        forceVerification.GradeConclusion
                    } : null,
                    ExtensometerVerification = extVerification != null ? new
                    {
                        extVerification.ReportId,
                        extVerification.CalibrationDate,
                        extVerification.Passed,
                        extVerification.GradeConclusion
                    } : null
                };
            }

            // 添加事件日志
            if (includeEventLog)
            {
                reportContent["EventLog"] = testRunContext.Events.Select(e => new
                {
                    e.Timestamp,
                    e.Source,
                    e.Message,
                    Level = e.Level.ToString()
                }).ToList();
            }

            // 添加数据文件引用
            var acquisitionResult = testRunContext.GetStepOutput<AcquisitionResult>("AcquisitionResult");
            reportContent["DataFiles"] = new
            {
                Files = acquisitionResult?.DataFiles ?? new List<string>(),
                TotalRecords = acquisitionResult?.TotalRecords ?? 0,
                DroppedSamples = acquisitionResult?.DroppedSamples ?? 0
            };

            // TODO: 根据实际格式生成报告
            // 目前仅支持 JSON 格式
            if (format == "JSON")
            {
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var jsonContent = JsonSerializer.Serialize(reportContent, jsonOptions);
                await File.WriteAllTextAsync(reportPath, jsonContent, context.CancellationToken);
            }
            else
            {
                // TODO: 实现其他格式的报告生成
                // 对于 PDF/Word/Excel/HTML，需要使用相应的库
                testRunContext.AddEvent("GenerateReportStep",
                    $"报告格式 {format} 暂未完全实现，已生成 JSON 备用", EventLevel.Warning);
                reportPath = Path.ChangeExtension(reportPath, ".json");
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var jsonContent = JsonSerializer.Serialize(reportContent, jsonOptions);
                await File.WriteAllTextAsync(reportPath, jsonContent, context.CancellationToken);
            }

            testRunContext.AddEvent("GenerateReportStep", $"试验报告已生成: {reportPath}");

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["ReportPath"] = reportPath,
                ["ReportFormat"] = format,
                ["Template"] = template,
                ["SummaryMetrics"] = summaryMetrics
            });
        }
    }
}
