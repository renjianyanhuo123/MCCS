namespace MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters
{
    /// <summary>
    /// 控制通道选项项
    /// </summary>
    public record ControlChannelItem
    {
        /// <summary>
        /// 通道ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 通道内部ID
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;
    }
}
