using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;

namespace MCCS.Common.DataManagers.Devices
{
    /// <summary>
    /// 作动器设备
    /// </summary>
    public class ActuatorDevice : BaseDevice
    {
        public ActuatorDevice(long id, string name) : base(id, name, DeviceTypeEnum.Actuator)
        {
            ValveStatus = ValveStatusEnum.Closed; 
        }

        /// <summary>
        /// 阀门状态
        /// </summary>
        public ValveStatusEnum? ValveStatus { get; private set; }
        /// <summary>
        /// 开阀
        /// </summary>
        public void OpenValve()
        {
            ValveStatus = ValveStatusEnum.Opened;
        }
        /// <summary>
        /// 关阀
        /// </summary>
        public void CloseValve()
        {
            ValveStatus = ValveStatusEnum.Closed;
        }
    }
}
