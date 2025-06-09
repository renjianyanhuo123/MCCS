using MCCS.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.DevicesService
{
    /// <summary>
    /// Manages device connections within the MCCS application.
    /// 注意: 管理多个设备连接的逻辑需要在此类中实现。
    /// </summary>
    public class DeviceConnectionManager : IDeviceConnectionManager
    {
        private readonly IDeviceInfoRepository _deviceInfoRepository;

        public DeviceConnectionManager(IDeviceInfoRepository deviceInfoRepository)
        {
            _deviceInfoRepository = deviceInfoRepository ?? throw new ArgumentNullException(nameof(deviceInfoRepository));
        }
    }
}
