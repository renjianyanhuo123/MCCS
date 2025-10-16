namespace MCCS.Collecter.HardwareDevices
{
    public enum HardwareConnectionStatus
    {
        /// <summary>
        /// 已连接
        /// </summary>
        Connected,
        /// <summary>
        /// 已断开
        /// </summary>
        Disconnected,
        /// <summary>
        /// 连接错误
        /// </summary>
        Error,
        /// <summary>
        /// 标定中
        /// </summary>
        Calibrating
    }
}
