namespace MCCS.Core.Models.SystemManager
{
    public enum HardwareTypeEnum : int
    {
        /// <summary>
        /// 控制器类
        /// </summary>
        Controller,
        /// <summary>
        /// 传感器类
        /// </summary>
        Sensor,
        /// <summary>
        /// 作动器 / 执行器类设备
        /// </summary>
        Actuator,
        /// <summary>
        /// 能源供应类
        /// </summary>
        EnergySupply,
        /// <summary>
        /// 通讯类设备
        /// </summary>
        Communication
    }
}
