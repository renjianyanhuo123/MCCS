using MCCS.Core.Devices.Connections;
using MCCS.Core.Devices.Details;
using MCCS.Core.Models.Devices;

namespace MCCS.Core.Devices
{
    public class DeviceFactory : IDeviceFactory
    {
        private readonly IConnectionManager _connectionManager;

        public DeviceFactory(
            IConnectionManager connectionManager) 
        {
            _connectionManager = connectionManager;
        }

        public IDevice CreateDevice(DeviceInfo deviceInfo, bool isMock = true)
        {
            IDeviceConnection? deviceConnection = isMock ? _connectionManager.GetConnection("Mock")
                    : _connectionManager.GetConnection(deviceInfo.MainDeviceId ?? "");
            switch (deviceInfo.DeviceType)
            {
                case DeviceTypeEnum.Unknown:
                    break;
                case DeviceTypeEnum.Sensor:
                    break;
                case DeviceTypeEnum.Actuator:
                    break;
                case DeviceTypeEnum.Controller:
                    break;
                case DeviceTypeEnum.Gateway:
                    break;
                case DeviceTypeEnum.Display:
                    break;
                case DeviceTypeEnum.Other:
                    break;
                default:
                    break;
            }
            if (deviceConnection == null)
                throw new ArgumentNullException(nameof(deviceConnection));
            return new Actuator(deviceInfo, deviceConnection);
        }
    }
}
