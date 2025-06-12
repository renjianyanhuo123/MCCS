using MCCS.Core.Devices;
using MCCS.Services.CollectionService;
using MCCS.Services.DevicesService;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.Coordinators
{
    public class DeviceSystemCoordinator : IDeviceSystemCoordinator
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDataAcquisitionManager _dataAcquisitionManager;
        private readonly IDeviceConnectionFactory _connectionFactory;
        private readonly CompositeDisposable _disposables = [];

        // 系统事件流
        //public IObservable<SystemEvent> SystemEvents { get; }
        //private readonly Subject<SystemEvent> _systemEventSubject = new();

        /*
         * 
        // 报警流
        public IObservable<AlarmEvent> Alarms { get; }
        
        // 系统健康状态流
        public IObservable<SystemHealth> HealthStream { get; }
        
        // 指令执行历史流
        public IObservable<CommandExecutionHistory> CommandHistory { get; }
         */


        public DeviceSystemCoordinator(
            IDeviceManager deviceManager,
            IDataAcquisitionManager dataAcquisitionManager,
            IDeviceConnectionFactory connectionFactory)
        {
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            _dataAcquisitionManager = dataAcquisitionManager ?? throw new ArgumentNullException(nameof(dataAcquisitionManager));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        }
    }
}
