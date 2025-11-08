using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;
using MCCS.Infrastructure.TestModels;

namespace MCCS.Common.DataManagers.Devices
{
    /// <summary>
    /// 作动器设备
    /// </summary>
    public class ActuatorDevice : BaseDevice
    {
        public ActuatorDevice(long id, string name, long? parentDevice) : base(id, name, DeviceTypeEnum.Actuator, parentDevice)
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
        public void OperationValve(bool isOpen)
        {
            ValveStatus = isOpen ?  ValveStatusEnum.Opened : ValveStatusEnum.Closed;
        }
    }
}
