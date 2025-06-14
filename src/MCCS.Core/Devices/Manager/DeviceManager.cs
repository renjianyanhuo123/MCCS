using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    public sealed class DeviceManager : IDeviceManager
    {
        private readonly ConcurrentDictionary<string, IDevice> _devices = new(); 
        private readonly IDeviceFactory _deviceFactory;
        private readonly IDeviceInfoRepository _deviceInfoRepository;

        public DeviceManager( 
            IDeviceFactory deviceFactory,
            IDeviceInfoRepository deviceInfoRepository)
        { 
            _deviceFactory = deviceFactory;
            _deviceInfoRepository = deviceInfoRepository
                ?? throw new ArgumentNullException(nameof(deviceInfoRepository));
        }

        public async Task<bool> RegisterDeviceFromRepository() 
        {
            var devices = await _deviceInfoRepository.GetAllDevicesAsync();
            if (devices == null || !devices.Any()) return false;
            foreach (var deviceInfo in devices)
            {
                var device = _deviceFactory.CreateDevice(deviceInfo);
                if (device != null)
                {
                    _devices.TryAdd(deviceInfo.DeviceId, device);
                }
            }
            return true;
        }

        public void RegisterDevice(DeviceInfo deviceInfo)
        {
            var device = _deviceFactory.CreateDevice(deviceInfo);
            if (device == null) return;
            _devices.TryAdd(device.Id, device);
        }

        public IDevice? GetDevice(string deviceId)
        {
            return _devices.TryGetValue(deviceId, out var device) ? device : null;
        }

        public void StartAllDevices(TimeSpan? timeSpan = null)
        {
            foreach (var device in _devices.Values)
            {
                device.SetSamplingInterval(timeSpan ?? TimeSpan.FromSeconds(1));
                device.Start();
            }
        }

        public void StopAllDevices()
        {
            foreach (var device in _devices.Values)
            {
                device.Stop();
            }
        }

        public void StartDevice(string deviceId, TimeSpan? timeSpan = null)
        {
            var device = GetDevice(deviceId);
            if (device == null) return;
            device.SetSamplingInterval(timeSpan ?? TimeSpan.FromSeconds(1));
            device.Start();
        }

        public void StopDevice(string deviceId)
        {
            GetDevice(deviceId)?.Stop();
        }

        public void Dispose()
        {
            foreach (var device in _devices.Values)
            {
                device.Dispose();
            }
            _devices.Clear();
        }
    }
}
