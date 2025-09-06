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

        public IDevice CreateDevice(DeviceInfo deviceInfo)
        {
            //var deviceConnection = _connectionManager.GetConnection(deviceInfo.MainDeviceId ?? "") 
            //    ?? throw new ArgumentNullException(nameof(deviceInfo.MainDeviceId));
            //switch (deviceInfo.DeviceType)
            //{
            //    case DeviceTypeEnum.Unknown:
            //        //throw new NotSupportedException();
            //    case DeviceTypeEnum.Sensor:
            //        break;
            //        //throw new NotSupportedException();
            //    case DeviceTypeEnum.Actuator:
            //        return new Actuator(deviceInfo, deviceConnection);
            //    case DeviceTypeEnum.Controller:
            //        //throw new NotSupportedException();
            //    case DeviceTypeEnum.Gateway:
            //        //throw new NotSupportedException();
            //    case DeviceTypeEnum.Display:
            //        //throw new NotSupportedException();
            //    case DeviceTypeEnum.Other:
            //        //throw new NotSupportedException();
            //    default:
            //        break;
            //        //throw new NotSupportedException();
            //}

            //return new Actuator(deviceInfo, deviceConnection);
            return null;
        }
    }
}
