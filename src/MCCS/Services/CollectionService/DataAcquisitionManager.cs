using MCCS.Core.Collection;
using MCCS.Core.Devices;
using MCCS.Core.Devices.Manager;
using MCCS.Core.Models.Devices;
using MCCS.Services.DevicesService;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.CollectionService
{
    /// <summary>
    /// 数据采集管理器
    /// </summary>
    public class DataAcquisitionManager
    {
        private readonly Dictionary<string, DataCollector> _collectors = [];
        private readonly IDeviceManager _deviceManager; 
        private readonly Subject<DataCollectionError> _errorSubject = new();

        /// <summary>
        /// 错误流
        /// </summary>
        public IObservable<DataCollectionError> ErrorStream =>  _errorSubject.AsObservable(); 

        public DataAcquisitionManager(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;

            // 监听设备状态变化，自动管理采集器
            _deviceManager.StatusChanges
            .Where(e => e.NewStatus == DeviceStatusEnum.Disconnected || e.NewStatus == DeviceStatusEnum.Error)
            .Subscribe(e => StopCollection(e.DeviceId)); 
        }

        public void StartCollection(string deviceId, TimeSpan? interval = null)
        {
            var device = _deviceManager.GetDevice(deviceId);
            if (device == null)
                throw new InvalidOperationException($"Device {deviceId} not found");

            if (_collectors.ContainsKey(deviceId))
                return;

            var collector = new DataCollector(device, interval ?? TimeSpan.FromSeconds(1)); 
            // 订阅错误流
            collector.ErrorStream.Subscribe(error =>
                _errorSubject.OnNext(new DataCollectionError
                {
                    DeviceId = deviceId,
                    Error = error,
                    Timestamp = DateTime.Now
                }));

            _collectors[deviceId] = collector;
            collector.Start();
        }

        public void StopCollection(string deviceId)
        {
            if (_collectors.TryGetValue(deviceId, out var collector))
            {
                collector.Stop();
                collector.Dispose();
                _collectors.Remove(deviceId);
            }
        }

        public void Dispose()
        {
            foreach (var collector in _collectors.Values)
            {
                collector.Dispose();
            }
            _collectors.Clear();
            _errorSubject.Dispose();
        }

        public void StartAllCollection(TimeSpan? interval = null)
        {
            throw new NotImplementedException();
        }

        public void StopAllCollection()
        {
            throw new NotImplementedException();
        }
    }
}
