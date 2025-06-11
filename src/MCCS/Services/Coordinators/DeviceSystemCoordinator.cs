using MCCS.Core.Devices;
using MCCS.Services.CollectionService;
using MCCS.Services.DevicesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.Coordinators
{
    public class DeviceSystemCoordinator : IDeviceSystemCoordinator
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDataAcquisitionManager _dataAcquisitionManager;
        private readonly IDeviceConnectionFactory _connectionFactory;
        private readonly CompositeDisposable _disposables = new();
    }
}
