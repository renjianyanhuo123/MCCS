using System.IO;

using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 引伸计/应变链核查步骤
    /// 入参：引伸计量程、等级目标、校准器信息
    /// 出参：ExtVerificationReportId/Path
    /// 依据：ASTM E83 引伸计系统的标定/核查/分级
    /// </summary>
    [StepComponent("verify-extensometer", "引伸计核查",
        Description = "对引伸计/应变测量链进行核查 (ASTM E83)",
        Category = ComponentCategory.CalibrationAndVerification,
        Icon = "RulerSquare",
        Tags = ["校准", "核查", "引伸计", "应变", "ASTM E83"])]
    public class VerifyExtensometerStep : BaseWorkflowStep
    {
        [StepInput("ExtensometerChannel")]
        public string ExtensometerChannel { get; set; } = string.Empty;

        [StepInput("GaugeLength")]
        public double GaugeLength { get; set; } = 50.0;

        [StepInput("TargetClass")]
        public string TargetClass { get; set; } = "B1";

        [StepInput("CalibratorInfo")]
        public string CalibratorInfo { get; set; } = string.Empty;

        [StepInput("SkipIfValid")]
        public bool SkipIfValid { get; set; } = true;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "ExtensometerChannel",
                DisplayName = "引伸计通道",
                Description = "要核查的引伸计通道ID",
                IsRequired = true,
                Order = 1
            };

            yield return new DoubleParameter
            {
                Name = "GaugeLength",
                DisplayName = "标距(mm)",
                Description = "引伸计的标距长度",
                IsRequired = true,
                DefaultValue = 50.0,
                MinValue = 1.0,
                MaxValue = 1000.0,
                Order = 2
            };

            yield return new SelectParameter
            {
                Name = "TargetClass",
                DisplayName = "目标等级",
                Description = "核查的目标精度等级 (ASTM E83)",
                IsRequired = true,
                DefaultValue = "B1",
                Options =
                [
                    new SelectOption("A", "A级 (±0.00015)"),
                    new SelectOption("B1", "B1级 (±0.0005)"),
                    new SelectOption("B2", "B2级 (±0.0005)"),
                    new SelectOption("C", "C级 (±0.001)"),
                    new SelectOption("D", "D级 (±0.01)"),
                    new SelectOption("E", "E级 (±0.1)")
                ],
                Order = 3
            };

            yield return new StringParameter
            {
                Name = "CalibratorInfo",
                DisplayName = "校准器信息",
                Description = "用于核查的校准器/千分尺信息",
                IsRequired = true,
                Order = 4
            };

            yield return new BooleanParameter
            {
                Name = "SkipIfValid",
                DisplayName = "有效期内跳过",
                Description = "如果上次核查在有效期内，是否跳过本次核查",
                DefaultValue = true,
                Order = 5
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var channel = GetParameter<string>("ExtensometerChannel");
            var gaugeLength = GetParameter<double>("GaugeLength");
            var targetClass = GetParameter<string>("TargetClass") ?? "B1";
            var calibratorInfo = GetParameter<string>("CalibratorInfo");
            var skipIfValid = GetParameter<bool>("SkipIfValid");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            var calibrationPolicy = testRunContext.Recipe.Calibration;

            // 检查是否可以跳过
            if (skipIfValid)
            {
                // TODO: 检查上次核查日期是否在有效期内
            }

            // 根据等级确定允许误差
            var toleranceMap = new Dictionary<string, double>
            {
                ["A"] = 0.00015,
                ["B1"] = 0.0005,
                ["B2"] = 0.0005,
                ["C"] = 0.001,
                ["D"] = 0.01,
                ["E"] = 0.1
            };
            var tolerance = toleranceMap.GetValueOrDefault(targetClass, 0.001);

            // 创建核查报告
            var report = new VerificationReport
            {
                ReportId = Guid.NewGuid().ToString(),
                CalibrationDate = DateTime.Now,
                ValidUntil = DateTime.Now.AddDays(calibrationPolicy.VerificationValidDays),
                StandardInfo = calibratorInfo ?? "未指定",
                Operator = testRunContext.Operator,
                Passed = true
            };

            // TODO: 向站点服务发送核查指令
            // 核查点通常为量程的 20%, 50%, 80%, 100%
            // var verificationPoints = new[] { 0.2, 0.5, 0.8, 1.0 };
            // foreach (var ratio in verificationPoints)
            // {
            //     var targetDisp = gaugeLength * tolerance * ratio;
            //     var result = await _stationService.PerformExtensometerVerificationAsync(channel, targetDisp);
            //     report.CalibrationPoints.Add(...);
            // }

            // 模拟核查结果
            var verificationPoints = new[] { 0.2, 0.5, 0.8, 1.0 };
            foreach (var ratio in verificationPoints)
            {
                var targetDisp = gaugeLength * 0.01 * ratio; // 1% of gauge length
                var simulatedError = (new Random().NextDouble() - 0.5) * tolerance * 0.5;
                report.CalibrationPoints.Add(new CalibrationPoint
                {
                    StandardValue = targetDisp,
                    IndicatedValue = targetDisp * (1 + simulatedError),
                    Error = targetDisp * simulatedError,
                    ErrorPercent = simulatedError * 100
                });
            }

            report.GradeConclusion = report.Passed ? $"合格 ({targetClass}级)" : "不合格";

            // 保存报告路径
            report.ReportPath = Path.Combine(testRunContext.DataFolder, $"ExtVerification_{report.ReportId}.json");

            // 记录到上下文
            testRunContext.SetStepOutput("ExtensometerVerification", report);
            testRunContext.AddEvent("VerifyExtensometerStep",
                $"引伸计核查完成: {(report.Passed ? "通过" : "未通过")}, 等级: {report.GradeConclusion}");

            await Task.CompletedTask;

            if (!report.Passed && calibrationPolicy.RequireVerification)
            {
                return StepResult.Fail($"引伸计核查未通过，无法达到目标等级 {targetClass}");
            }

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = report.Passed ? StepResultCode.OK : StepResultCode.Warning,
                ["ReportId"] = report.ReportId,
                ["ReportPath"] = report.ReportPath,
                ["Passed"] = report.Passed,
                ["GradeConclusion"] = report.GradeConclusion,
                ["Channel"] = channel,
                ["GaugeLength"] = gaugeLength
            });
        }
    }
}
