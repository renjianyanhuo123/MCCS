namespace MCCS.Station.Abstractions.Enums
{
    public enum DeviceStatusEnum : int 
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 1,
        /// <summary>
        /// 离线
        /// </summary>
        Disconnected = 2,
        /// <summary>
        /// 错误 异常
        /// </summary>
        Error = 3,
        /// <summary>
        /// 繁忙
        /// </summary>
        Busy = 4,
        /// <summary>
        /// 无效
        /// </summary>
        Disabled = 5,
        /// <summary>
        /// 连接中
        /// </summary>
        Connecting
    }
}
