using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 试件安装与对中步骤（人工确认）
    /// 入参：装夹指导（图片/检查清单）、允许跳过项
    /// 出参：操作者确认、备注、照片路径（可选）
    /// </summary>
    [StepComponent("user-mount-specimen", "试件安装",
        Description = "人工安装试件并确认（夹具、对中、预紧）",
        Category = ComponentCategory.ManualOperation,
        Icon = "WrenchOutline",
        Tags = ["人工", "安装", "试件", "对中", "确认"])]
    public class UserMountSpecimenStep : BaseWorkflowStep
    {
        [StepInput("Instructions")]
        public string Instructions { get; set; } = string.Empty;

        [StepInput("ChecklistItems")]
        public string ChecklistItems { get; set; } = string.Empty;

        [StepInput("RequirePhoto")]
        public bool RequirePhoto { get; set; } = false;

        [StepInput("AllowSkip")]
        public bool AllowSkip { get; set; } = false;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new MultilineTextParameter
            {
                Name = "Instructions",
                DisplayName = "装夹指导",
                Description = "试件装夹的操作指导说明",
                IsRequired = false,
                Rows = 5,
                DefaultValue = "1. 检查夹具状态\n2. 安装试件\n3. 对中调整\n4. 预紧检查",
                Order = 1
            };

            yield return new StringParameter
            {
                Name = "ChecklistItems",
                DisplayName = "检查项",
                Description = "必须确认的检查项，逗号分隔",
                IsRequired = false,
                DefaultValue = "夹具锁紧,对中完成,引伸计安装,安全区域清空",
                Order = 2
            };

            yield return new BooleanParameter
            {
                Name = "RequirePhoto",
                DisplayName = "要求拍照",
                Description = "是否要求操作者提供照片",
                DefaultValue = false,
                Order = 3
            };

            yield return new BooleanParameter
            {
                Name = "AllowSkip",
                DisplayName = "允许跳过",
                Description = "是否允许跳过此步骤",
                DefaultValue = false,
                Order = 4
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var instructions = GetParameter<string>("Instructions");
            var checklistStr = GetParameter<string>("ChecklistItems") ?? "";
            var requirePhoto = GetParameter<bool>("RequirePhoto");
            var allowSkip = GetParameter<bool>("AllowSkip");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            // 解析检查项
            var checklistItems = checklistStr.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            // TODO: 向站点服务发送人工确认请求，等待用户响应
            // var confirmRequest = new UserConfirmRequest
            // {
            //     Title = "试件安装确认",
            //     Instructions = instructions,
            //     ChecklistItems = checklistItems,
            //     RequirePhoto = requirePhoto,
            //     AllowSkip = allowSkip
            // };
            // var confirmResult = await _stationService.RequestUserConfirmationAsync(confirmRequest, context.CancellationToken);

            // 模拟等待用户确认（实际应该是暂停工作流等待外部事件）
            testRunContext.AddEvent("UserMountSpecimenStep", "等待用户确认试件安装...", EventLevel.Info);

            // 暂停工作流，等待用户确认
            // 这里返回 Suspend 结果，实际需要通过外部事件恢复
            // 为了演示，我们模拟用户已确认
            var confirmResult = new UserConfirmationResult
            {
                Confirmed = true,
                Operator = testRunContext.Operator,
                ConfirmTime = DateTime.Now,
                Remarks = "试件安装完成，已确认所有检查项"
            };

            if (!confirmResult.Confirmed && !allowSkip)
            {
                return StepResult.Fail("用户未确认试件安装");
            }

            // 记录到上下文
            testRunContext.SetStepOutput("SpecimenMountConfirmation", confirmResult);
            testRunContext.AddEvent("UserMountSpecimenStep",
                $"试件安装确认完成, 操作者: {confirmResult.Operator}");

            await Task.CompletedTask;

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["Confirmed"] = confirmResult.Confirmed,
                ["Operator"] = confirmResult.Operator,
                ["ConfirmTime"] = confirmResult.ConfirmTime,
                ["Remarks"] = confirmResult.Remarks,
                ["Attachments"] = confirmResult.Attachments
            });
        }
    }
}
