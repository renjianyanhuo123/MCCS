using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;

namespace MCCS.Core.Devices;

/// <summary>
/// 设备基类 - 封装通用逻辑
/// </summary>
public abstract class BaseDevice : IDevice
{
    private readonly Channel<DeviceData> _dataChannel;
    protected readonly Subject<CommandResponse> _commandResponseSubject = new();
    protected readonly IDeviceConnection _connection;
    private readonly MCCS.Core.Devices.Connections.AsyncLock _sendLock = new();
    private IDisposable _dataSubscription;
    private TimeSpan _samplingInterval = TimeSpan.Zero; // 默认不限制频率

    public string Id { get; }

    public string ConnectionId { get; }

    public bool IsActive { get; private set; }

    public string Name { get; }
    public DeviceTypeEnum Type { get; }

    public IObservable<DeviceData> DataStream { get; }

    protected BaseDevice(
        string id, 
        string name, 
        DeviceTypeEnum type,
        IDeviceConnection connection)
    {
        Id = id;
        Name = name;
        Type = type;
        ConnectionId = connection.ConnectionId;
        _connection = connection;
        _dataChannel = Channel.CreateUnbounded<DeviceData>(); 
        DataStream = Observable.Create<DeviceData>(async (observer, ct) =>
        {
            await foreach (var data in _dataChannel.Reader.ReadAllAsync(ct))
                observer.OnNext(data);
        });
    }

    public virtual void Start()
    {
        if (IsActive) return;

        IsActive = true;
        // 每个设备独立订阅连接的数据流，自行过滤属于自己的数据
        var dataStream = _connection.DataReceived
            .Where(data => IsDeviceData(data))
            .Select(data => ProcessData(data))
            .Where(data => data != null);
        // 如果设置了采样间隔，使用Sample操作符控制频率
        if (_samplingInterval != TimeSpan.Zero)
        {
            dataStream = dataStream.Sample(_samplingInterval);
        }
        _dataSubscription = dataStream.Subscribe(data => _dataChannel.Writer.TryWrite(data));
    }

    public virtual void Stop()
    {
        IsActive = false;
        _dataSubscription?.Dispose();
    }

    protected abstract bool IsDeviceData(byte[] data);
    protected abstract DeviceData ProcessData(byte[] rawData);
    protected abstract byte[] PrepareCommand(DeviceCommand command);

    public virtual async Task<CommandResponse> SendCommandAsync(DeviceCommand command)
    {
        var response = new CommandResponse
        {
            CommandId = command.CommandId,
            DeviceId = Id
        };

        try
        {
            using (await _sendLock.LockAsync()) // 多设备共享连接时防止命令冲突
            {
                // 调用具体设备的指令处理
                var commandData = PrepareCommand(command);
                await _connection.SendCommandAsync(commandData);
            }
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
            return response;
        }
    }

    public virtual void SetSamplingInterval(TimeSpan interval)
    {
        _samplingInterval = interval;

        // 如果设备正在运行，重新启动以应用新的采样间隔
        if (IsActive)
        {
            Stop();
            Start();
        }
    }

    public virtual void Dispose()
    {
        Stop();
        _dataChannel.Writer.TryComplete();
    }
}