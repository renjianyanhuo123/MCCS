using MCCS.Core.Devices.Details;
using MCCS.Core.Devices.Mocks;
using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices
{
    public class DeviceFactory : IDeviceFactory
    {
        private readonly IDeviceConnectionFactory _deviceConnectionFactory;

        public DeviceFactory(IDeviceConnectionFactory deviceConnectionFactory) 
        {
            _deviceConnectionFactory = deviceConnectionFactory;
        }

        public IDevice CreateDevice(DeviceInfo deviceInfo, bool isMock = true)
        {
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
            return new Actuator(deviceInfo, _deviceConnectionFactory, isMock);
        }
    }
}
