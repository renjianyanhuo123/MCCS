namespace MCCS.Events.Tests
{
    public record NotificationRightMenuValveStatusEventParam
    {
        public long DeviceId { get; init; }
        /// <summary>
        /// 用于回传; 更新模型上界面的显示
        /// </summary>
        public required string ModelId { get; init; }
    }
}
