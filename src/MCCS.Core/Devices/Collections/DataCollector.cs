using MCCS.Core.Devices.Manager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Collections
{
    public sealed class DataCollector : IDataCollector
    {
        private readonly IDeviceManager _deviceManager;
        private readonly Subject<DeviceData> _allDataSubject = new();
        private readonly ConcurrentDictionary<string, IDisposable> _subscriptions = new();

        public IObservable<DeviceData> AllDataStream => _allDataSubject.AsObservable();

        public DataCollector(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public IObservable<DeviceData> GetAllDataStreams()
        {
            return AllDataStream;
        }

        public IObservable<DeviceData> GetDataStream(string deviceId)
        {
            var device = _deviceManager.GetDevice(deviceId);
            return device?.DataStream 
                ?? Observable.Empty<DeviceData>();
        }

        public void StartCollection(TimeSpan? timeSpan = null)
        {
            //_deviceManager.StartAllDevices(timeSpan);
        }

        public void StopCollection()
        {
            _deviceManager.StopAllDevices();
        }

        public void SubscribeToDevice(string deviceId)
        { 
        }

        public void UnsubscribeFromDevice(string deviceId)
        {
            if (_subscriptions.TryRemove(deviceId, out var subscription))
            {
                subscription.Dispose();
            }
        }

        public void Dispose()
        { 
        }
    }
}
