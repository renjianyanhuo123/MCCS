using System.Windows;
using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Components
{
    /// <summary>
    /// 消息框组件 - 显示消息对话框
    /// </summary>
    [StepComponent("message-box", "消息提示",
        Description = "显示消息对话框，可用于提示用户或等待用户确认",
        Category = ComponentCategory.UserInteraction,
        Icon = "MessageAlert",
        Tags = new[] { "消息", "提示", "对话框", "弹窗" })]
    public class MessageBoxComponent : BaseStepComponent
    {
        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "Title",
                DisplayName = "标题",
                Description = "消息框的标题",
                IsRequired = true,
                DefaultValue = "提示",
                Placeholder = "请输入标题",
                Order = 1
            };

            yield return new MultilineTextParameter
            {
                Name = "Message",
                DisplayName = "消息内容",
                Description = "要显示的消息内容，支持变量引用",
                IsRequired = true,
                Placeholder = "请输入消息内容...",
                Rows = 3,
                Order = 2
            };

            yield return new SelectParameter
            {
                Name = "MessageType",
                DisplayName = "消息类型",
                Description = "消息框的图标类型",
                DefaultValue = "Information",
                Options = new List<SelectOption>
                {
                    new("None", "无图标"),
                    new("Information", "信息"),
                    new("Warning", "警告"),
                    new("Error", "错误"),
                    new("Question", "询问")
                },
                Order = 3
            };

            yield return new SelectParameter
            {
                Name = "Buttons",
                DisplayName = "按钮类型",
                Description = "消息框显示的按钮",
                DefaultValue = "OK",
                Options = new List<SelectOption>
                {
                    new("OK", "确定"),
                    new("OKCancel", "确定/取消"),
                    new("YesNo", "是/否"),
                    new("YesNoCancel", "是/否/取消")
                },
                Order = 4
            };
        }

        protected override Task<ComponentExecutionResult> ExecuteCoreAsync(
            ComponentExecutionContext context,
            CancellationToken cancellationToken)
        {
            var title = GetParameterValue<string>("Title") ?? "提示";
            var message = GetParameterValue<string>("Message") ?? string.Empty;
            var messageType = GetParameterValue<string>("MessageType") ?? "Information";
            var buttons = GetParameterValue<string>("Buttons") ?? "OK";

            // 替换变量
            title = ReplaceVariables(title, context);
            message = ReplaceVariables(message, context);

            context.Log?.Invoke($"显示消息框: {title}", LogLevel.Info);

            // 在UI线程上显示消息框
            MessageBoxResult result = MessageBoxResult.None;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var icon = messageType switch
                {
                    "Information" => MessageBoxImage.Information,
                    "Warning" => MessageBoxImage.Warning,
                    "Error" => MessageBoxImage.Error,
                    "Question" => MessageBoxImage.Question,
                    _ => MessageBoxImage.None
                };

                var btns = buttons switch
                {
                    "OKCancel" => MessageBoxButton.OKCancel,
                    "YesNo" => MessageBoxButton.YesNo,
                    "YesNoCancel" => MessageBoxButton.YesNoCancel,
                    _ => MessageBoxButton.OK
                };

                result = MessageBox.Show(message, title, btns, icon);
            });

            var resultStr = result.ToString();
            context.Log?.Invoke($"用户选择: {resultStr}", LogLevel.Info);

            return Task.FromResult(ComponentExecutionResult.Success(new Dictionary<string, object?>
            {
                ["Result"] = resultStr,
                ["IsConfirmed"] = result is MessageBoxResult.OK or MessageBoxResult.Yes,
                ["IsCancelled"] = result is MessageBoxResult.Cancel or MessageBoxResult.No
            }));
        }

        private static string ReplaceVariables(string template, ComponentExecutionContext context)
        {
            var result = template;

            foreach (var kvp in context.GlobalVariables)
            {
                result = result.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            foreach (var kvp in context.LocalVariables)
            {
                result = result.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        public override IStepComponent Clone()
        {
            var clone = new MessageBoxComponent();
            clone.SetParameterValues(GetParameterValues());
            return clone;
        }
    }
}
