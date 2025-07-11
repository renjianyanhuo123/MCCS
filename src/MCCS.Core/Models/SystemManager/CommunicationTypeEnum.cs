namespace MCCS.Core.Models.SystemManager
{
    public enum CommunicationTypeEnum : int
    {
        /// <summary>
        /// 串行通讯
        /// </summary>
        Serial,
        /// <summary>
        /// 以太网通讯
        /// </summary>
        Ethernet,
        /// <summary>
        /// 总线协议
        /// </summary>
        Bus,
        /// <summary>
        /// 无线通讯
        /// </summary>
        Wireless,
        /// <summary>
        /// 其他
        /// </summary>
        Other
    }
}
