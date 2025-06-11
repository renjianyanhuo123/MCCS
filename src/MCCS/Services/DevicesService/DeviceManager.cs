using MCCS.Core.Devices;
using MCCS.Core.Devices.Manager;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using SharpDX.Direct3D9;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Services.DevicesService
{
    /// <summary>
    /// Manages device connections within the MCCS application.
    /// 注意: 管理多个设备连接的逻辑需要在此类中实现。
    /// </summary>
    public class DeviceManager : IDeviceManager
    {
        private readonly Dictionary<string, IDevice> _devices = [];
        private readonly IDeviceConnectionFactory _connectionFactory;
        private readonly IDeviceInfoRepository _deviceInfoRepository;

        // 使用Subject发布设备事件
        private readonly Subject<DeviceEvent> _deviceEventSubject = new();

        // 公开的事件流
        public IObservable<DeviceEvent> DeviceEvents => _deviceEventSubject.AsObservable();

        // 设备状态变化流
        public IObservable<DeviceStatusEvent> StatusChanges =>
            DeviceEvents.OfType<DeviceStatusEvent>();

        // 设备添加/移除流
        public IObservable<DeviceRegistrationEvent> RegistrationChanges =>
            DeviceEvents.OfType<DeviceRegistrationEvent>();

        // 所有设备的状态聚合流
        public IObservable<Dictionary<string, DeviceStatusEnum>> AllDeviceStatuses { get; }

        public DeviceManager(
            IDeviceConnectionFactory connectionFactory,
            IDeviceInfoRepository deviceInfoRepository)
        {
            _connectionFactory = connectionFactory 
                ?? throw new ArgumentNullException(nameof(connectionFactory));
            _deviceInfoRepository = deviceInfoRepository 
                ?? throw new ArgumentNullException(nameof(deviceInfoRepository));
            // 创建所有设备状态的聚合流
            AllDeviceStatuses = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => _devices.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.StatusStream.FirstAsync().Wait()
                ))
                .StartWith(new Dictionary<string, DeviceStatusEnum>())
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// 注册设备
        /// </summary>
        /// <param name="device"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RegisterDevice(IDevice device)
        {
            if (_devices.ContainsKey(device.Id))
                throw new InvalidOperationException($"Device {device.Id} already registered");

            _devices[device.Id] = device;

            // 订阅设备状态变化
            device.StatusStream
                .DistinctUntilChanged()
                .Scan(
                    new { Previous = DeviceStatusEnum.Disconnected, Current = DeviceStatusEnum.Disconnected },
                    (acc, current) => new { Previous = acc.Current, Current = current }
                )
                .Skip(1) // 跳过初始状态
                .Subscribe(state =>
                {
                    _deviceEventSubject.OnNext(new DeviceStatusEvent
                    {
                        DeviceId = device.Id,
                        OldStatus = state.Previous,
                        NewStatus = state.Current,
                        Timestamp = DateTime.Now
                    });
                });

            // 发布设备注册事件
            _deviceEventSubject.OnNext(new DeviceRegistrationEvent
            {
                DeviceId = device.Id,
                Type = RegistrationType.Added,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 卸载设备
        /// </summary>
        /// <param name="deviceId"></param>
        public void UnregisterDevice(string deviceId)
        {
            if (_devices.TryGetValue(deviceId, out var device))
            {
                device.DisconnectAsync().Wait();
                _devices.Remove(deviceId);

                _deviceEventSubject.OnNext(new DeviceRegistrationEvent
                {
                    DeviceId = deviceId,
                    Type = RegistrationType.Removed,
                    Timestamp = DateTime.Now
                });
            }
        }

        public IDevice? GetDevice(string deviceId)
        {
            return _devices.TryGetValue(deviceId, out var device) ? device : null;
        }

        public IObservable<IDevice?> GetDeviceStream(string deviceId)
        {
            return Observable
                .Return(GetDevice(deviceId))
                .Where(d => d != null);
        }

        public IObservable<IEnumerable<IDevice>> GetAllDevicesStream()
        {
            return Observable.Return(_devices.Values.AsEnumerable());
        }

        public void Dispose()
        {
            foreach (var device in _devices.Values)
            {
                if (device is IDisposable disposable)
                    disposable.Dispose();
            }
            _devices.Clear();
            _deviceEventSubject.Dispose();
        }
    }
}
