using MCCS.Core.Devices;
using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Manager;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using SharpDX.Direct3D9;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Services.DevicesService
{
    /// <summary>
    /// 注意: 管理多个设备连接的逻辑需要在此类中实现。
    /// </summary>
    public sealed class DeviceManager : IDeviceManager
    {
        private readonly ConcurrentDictionary<string, IDevice> _devices = [];
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IDeviceFactory _deviceFactory;
        private readonly CompositeDisposable _disposables = [];
        private readonly Subject<CommandResponse> _commandExecutionSubject = new();

        /// <summary>
        /// 使用Subject发布设备事件
        /// </summary>
        private readonly Subject<DeviceEvent> _deviceEventSubject = new();

        /// <summary>
        /// 公开的事件流，供外部订阅设备事件
        /// </summary>
        public IObservable<DeviceEvent> DeviceEvents => _deviceEventSubject.AsObservable();

        /// <summary>
        /// 设备状态变化流
        /// </summary>
        public IObservable<DeviceStatusEvent> StatusChanges =>
            DeviceEvents.OfType<DeviceStatusEvent>();

        /// <summary>
        /// 设备添加/移除流
        /// </summary>
        public IObservable<DeviceRegistrationEvent> RegistrationChanges =>
            DeviceEvents.OfType<DeviceRegistrationEvent>();

        /// <summary>
        /// 所有设备的状态聚合流
        /// </summary>
        public IObservable<Dictionary<string, DeviceStatusEnum>> AllDeviceStatuses { get; }

        /// <summary>
        /// 指令执行流
        /// </summary>
        public IObservable<CommandResponse> CommandExecutions { get; }

        public DeviceManager(
            IDeviceFactory deviceFactory,
            IDeviceInfoRepository deviceInfoRepository)
        {
            _deviceInfoRepository = deviceInfoRepository 
                ?? throw new ArgumentNullException(nameof(deviceInfoRepository));
            _deviceFactory = deviceFactory;
            CommandExecutions = _commandExecutionSubject.AsObservable();
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

        public async Task<bool> RegisterAllDeviceFromRepository()
        {
            var devices = await _deviceInfoRepository.GetAllDevicesAsync();
            if (devices == null || !devices.Any())
                return false;
            foreach (var deviceInfo in devices) 
            {
                var device = _deviceFactory.CreateDevice(deviceInfo);
                if (device != null)
                {
                    RegisterDevice(device);
                }
            }
            return true;
        }

        /// <summary>
        /// 注册单个设备
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

            // 汇总各个设备 订阅设备指令响应
            device.CommandResponseStream
                .Subscribe(response => _commandExecutionSubject.OnNext(response));

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
        public async Task UnregisterDevice(string deviceId)
        {
            if (_devices.TryGetValue(deviceId, out var device))
            {
                var isSuccess = await device.DisconnectAsync();
                if (device is IDisposable disposable && isSuccess)
                    disposable.Dispose();
                _devices.Remove(deviceId, out var deviceTemp);
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

        public async Task<CommandResponse> SendCommandAsync(DeviceCommand command)
        {
            var device = GetDevice(command.DeviceId);
            if (device == null)
                throw new InvalidOperationException($"Device {command.DeviceId} not found");

            return await device.SendCommandAsync(command);
        }


        public IObservable<CommandResponse> SendCommandsAsync(IEnumerable<DeviceCommand> commands)
        {
            return commands.ToObservable()
                .SelectMany(async cmd =>
                {
                    try
                    {
                        return await SendCommandAsync(cmd);
                    }
                    catch (Exception ex)
                    {
                        return new CommandResponse
                        {
                            CommandId = cmd.CommandId,
                            DeviceId = cmd.DeviceId,
                            Success = false,
                            ErrorMessage = ex.Message
                        };
                    }
                });
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
            _commandExecutionSubject.Dispose();
        } 
    }
}
