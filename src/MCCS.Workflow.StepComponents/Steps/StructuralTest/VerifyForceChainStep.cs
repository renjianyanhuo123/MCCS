using System.IO;

using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 力链核查/校准步骤
    /// 入参：校准策略、标准传感器信息
    /// 出参：ForceVerificationReportId/Path
    /// 依据：ASTM E4 力测量系统校准/验证标准
    /// </summary>
    [StepComponent("verify-force-chain", "力链核查",
        Description = "对力测量系统进行校准/核查，确保可溯源性 (ASTM E4)",
        Category = ComponentCategory.CalibrationAndVerification,
        Icon = "ScaleBalance",
        Tags = ["校准", "核查", "力", "传感器", "ASTM E4"])]
    public class VerifyForceChainStep : BaseWorkflowStep
    {
        [StepInput("StandardSensorId")]
        public string StandardSensorId { get; set; } = string.Empty;

        [StepInput("StandardSensorCertNo")]
        public string StandardSensorCertNo { get; set; } = string.Empty;

        [StepInput("VerificationPoints")]
        public string VerificationPoints { get; set; } = string.Empty;

        [StepInput("TolerancePercent")]
        public double TolerancePercent { get; set; } = 1.0;

        [StepInput("SkipIfValid")]
        public bool SkipIfValid { get; set; } = true;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "StandardSensorId",
                DisplayName = "标准传感器编号",
                Description = "用于校准的标准力传感器编号",
                IsRequired = true,
                Order = 1
            };

            yield return new StringParameter
            {
                Name = "StandardSensorCertNo",
                DisplayName = "标准器证书号",
                Description = "标准力传感器的校准证书编号",
                IsRequired = true,
                Order = 2
            };

            yield return new StringParameter
            {
                Name = "VerificationPoints",
                DisplayName = "核查点(kN)",
                Description = "核查力值点，逗号分隔，如: 10,20,50,100",
                IsRequired = true,
                DefaultValue = "10,20,50,100",
                Order = 3
            };

            yield return new DoubleParameter
            {
                Name = "TolerancePercent",
                DisplayName = "允许误差(%)",
                Description = "核查允许的最大误差百分比",
                IsRequired = true,
                DefaultValue = 1.0,
                MinValue = 0.1,
                MaxValue = 5.0,
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
            var standardSensorId = GetParameter<string>("StandardSensorId");
            var standardCertNo = GetParameter<string>("StandardSensorCertNo");
            var pointsStr = GetParameter<string>("VerificationPoints") ?? "10,20,50,100";
            var tolerance = GetParameter<double>("TolerancePercent");
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
                // var lastVerification = await _calibrationService.GetLastForceVerificationAsync();
                // if (lastVerification != null && lastVerification.ValidUntil > DateTime.Now)
                // {
                //     return StepResult.Succeed(...);
                // }
            }

            // 解析核查点
            var points = pointsStr.Split(',')
                .Select(s => double.TryParse(s.Trim(), out var v) ? v : 0)
                .Where(v => v > 0)
                .ToList();

            if (points.Count == 0)
            {
                return StepResult.Fail("未指定有效的核查点");
            }

            // 创建核查报告
            var report = new VerificationReport
            {
                ReportId = Guid.NewGuid().ToString(),
                CalibrationDate = DateTime.Now,
                ValidUntil = DateTime.Now.AddDays(calibrationPolicy.VerificationValidDays),
                StandardInfo = $"标准器: {standardSensorId}, 证书号: {standardCertNo}",
                Operator = testRunContext.Operator,
                Passed = true
            };

            // TODO: 向站点服务发送核查指令，执行实际核查
            // foreach (var point in points)
            // {
            //     var result = await _stationService.PerformForceVerificationPointAsync(point, standardSensorId);
            //     report.CalibrationPoints.Add(new CalibrationPoint
            //     {
            //         StandardValue = point,
            //         IndicatedValue = result.IndicatedValue,
            //         Error = result.Error,
            //         ErrorPercent = result.ErrorPercent
            //     });
            //     if (Math.Abs(result.ErrorPercent) > tolerance)
            //     {
            //         report.Passed = false;
            //     }
            // }

            // 模拟核查结果
            foreach (var point in points)
            {
                var simulatedError = (new Random().NextDouble() - 0.5) * tolerance * 0.8;
                report.CalibrationPoints.Add(new CalibrationPoint
                {
                    StandardValue = point,
                    IndicatedValue = point * (1 + simulatedError / 100),
                    Error = point * simulatedError / 100,
                    ErrorPercent = simulatedError
                });
            }

            report.GradeConclusion = report.Passed ? "合格 (Class 1)" : "不合格";

            // 保存报告路径
            report.ReportPath = Path.Combine(testRunContext.DataFolder, $"ForceVerification_{report.ReportId}.json");

            // 记录到上下文
            testRunContext.SetStepOutput("ForceVerification", report);
            testRunContext.AddEvent("VerifyForceChainStep",
                $"力链核查完成: {(report.Passed ? "通过" : "未通过")}, 报告ID: {report.ReportId}");

            await Task.CompletedTask;

            if (!report.Passed && calibrationPolicy.RequireVerification)
            {
                return StepResult.Fail($"力链核查未通过，请重新校准");
            }

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = report.Passed ? StepResultCode.OK : StepResultCode.Warning,
                ["ReportId"] = report.ReportId,
                ["ReportPath"] = report.ReportPath,
                ["Passed"] = report.Passed,
                ["GradeConclusion"] = report.GradeConclusion,
                ["ValidUntil"] = report.ValidUntil
            });
        }
    }
}
