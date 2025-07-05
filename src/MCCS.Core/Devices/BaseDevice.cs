using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;

namespace MCCS.Core.Devices;

/// <summary>
/// 设备基类 - 封装通用逻辑
/// </summary>
public abstract class BaseDevice : IDevice
{

    private readonly IDeviceConnection _connection;
    protected readonly Subject<DeviceData> _dataSubject = new();
    protected readonly Subject<CommandResponse> _commandStatusSubject = new();
    private IDisposable? _collectionSubscription;

    public string Id { get; }

    public string ConnectionId { get; }

    public bool IsActive { get; private set; }

    public string Name { get; }
    public DeviceTypeEnum Type { get; }

    public IObservable<DeviceData> DataStream => _dataSubject.AsObservable();

    public IObservable<CommandResponse> CommandStatusStream => _commandStatusSubject.AsObservable();

    public DeviceInfo DeviceInfo { get; }

    public DeviceStatusEnum Status { get; }

    protected BaseDevice(
        DeviceInfo deviceInfo, 
        IDeviceConnection connection)
    {
        Id = deviceInfo.DeviceId;
        Name = deviceInfo.DeviceName;
        Type = deviceInfo.DeviceType;
        DeviceInfo = deviceInfo;
        _connection = connection;
        ConnectionId = connection.ConnectionId;
        Status = connection.IsConnected ? DeviceStatusEnum.Connected : DeviceStatusEnum.Disconnected;
    }

    public virtual void StartCollection()
    {
        if (!_connection.IsConnected || IsActive) return;
        var interval = TimeSpan.FromMilliseconds(1000.0 / DeviceInfo.Frequency);
        _collectionSubscription = Observable
                    .Interval(interval)
                    .SelectMany(_ => Observable.FromAsync(ReadDataAsync))
                    .Where(data => data != null)
                    .Subscribe(
                        data => 
                        {
                            _dataSubject.OnNext(data);
                            // Debug.WriteLine($"设备 {DeviceInfo.DeviceId} 采集数据: {data}");
                        },
                        error => Debug.WriteLine($"设备 {DeviceInfo.DeviceId} 采集错误: {error.Message}")
                    );
        IsActive = true;
    }

    protected abstract Task<DeviceData> ReadDataAsync();

    public virtual void StopCollection()
    {
        if (IsActive == false) return;
        _collectionSubscription?.Dispose();
        _collectionSubscription = null;
        IsActive = false; 
    }

    public abstract Task<CommandResponse> SendCommandAsync(DeviceCommand command, CancellationToken cancellationToken);
     

    public virtual void Dispose()
    { 
        StopCollection();
        _dataSubject.Dispose();
        _commandStatusSubject.Dispose();
    }
}