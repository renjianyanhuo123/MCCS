using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters
{
    /// <summary>
    /// 控制操作参数模型（单个通道配置）
    /// </summary>
    public record ControlOperationParamModel
    { 
        /// <summary>
        /// 控制通道ID
        /// </summary>
        public long ControlChannelId { get; set; }

        /// <summary>
        /// 控制通道名称
        /// </summary>
        public string ControlChannelName { get; set; } = string.Empty;

        /// <summary>
        /// 允许的控制模式列表
        /// </summary>
        public List<ControlModeTypeEnum> AllowedControlModes { get; set; } = [];
    }
}
