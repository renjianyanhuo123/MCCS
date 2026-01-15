using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters
{
    /// <summary>
    /// 控制操作参数模型
    /// </summary>
    public record ControlOperationParamModel
    {
        public long ControlChannelId { get; set; }

        public string ControlChannelName { get; set; } = string.Empty;
        /// <summary>
        /// 允许的控制模式列表
        /// </summary>
        public required List<ControlModeTypeEnum> AllowedControlModes { get; set; } 
    }
}
