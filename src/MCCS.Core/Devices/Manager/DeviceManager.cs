using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using System.Collections.Concurrent;

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
        public void RegisterDevices(IEnumerable<DeviceInfo> deviceInfo)
        {
            foreach (var info in deviceInfo)
            {
                RegisterDevice(info);
            }
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

        public void StartAllDevices()
        {
            foreach (var device in _devices.Values)
            {
                device.StartCollection();
            }
        }

        public void StopAllDevices()
        {
            foreach (var device in _devices.Values)
            {
                device.StopCollection();
            }
        }

        public void StartDevice(string deviceId)
        { 
            var device = GetDevice(deviceId);
            device?.StartCollection();
        }

        public void StopDevice(string deviceId)
        {
            var device = GetDevice(deviceId);
            device?.StopCollection();
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
