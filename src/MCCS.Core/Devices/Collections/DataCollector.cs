using MCCS.Core.Devices.Manager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Collections
{
    public sealed class DataCollector : IDataCollector
    {
        private readonly IDeviceManager _deviceManager;
        private readonly Channel<DeviceData> _allDataChannel;
        private readonly ConcurrentDictionary<string, IDisposable> _subscriptions = new();

        public IObservable<DeviceData> AllDataStream { get; }

        public DataCollector(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            _allDataChannel = Channel.CreateUnbounded<DeviceData>();

            AllDataStream = Observable.Create<DeviceData>(async (observer, ct) =>
            {
                await foreach (var data in _allDataChannel.Reader.ReadAllAsync(ct))
                    observer.OnNext(data);
            });
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
            _deviceManager.StartAllDevices(timeSpan);
        }

        public void StopCollection()
        {
            _deviceManager.StopAllDevices();
        }

        public void SubscribeToDevice(string deviceId)
        {
            var device = _deviceManager.GetDevice(deviceId);
            if (device != null)
            {
                var subscription = device.DataStream.Subscribe(data =>
                    _allDataChannel.Writer.TryWrite(data));
                _subscriptions.TryAdd(deviceId, subscription);
            }
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
            foreach (var subscription in _subscriptions.Values)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
            _allDataChannel.Writer.TryComplete();
        }
    }
}
