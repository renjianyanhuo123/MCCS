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
    protected readonly BehaviorSubject<DeviceStatusEnum> _statusSubject = new(DeviceStatusEnum.Disconnected);
    protected readonly Subject<CommandResponse> _commandResponseSubject = new();
    protected readonly IDeviceConnection _connection;

    public string Id { get; }
    public string Name { get; }
    public DeviceTypeEnum Type { get; }
    
    /// <summary>
    /// 公开状态流
    /// </summary>
    public IObservable<DeviceStatusEnum> StatusStream => _statusSubject.AsObservable();

    /// <summary>
    /// 公开指令响应流
    /// </summary>
    public IObservable<CommandResponse> CommandResponseStream => _commandResponseSubject.AsObservable();

    /// <summary>
    /// 当前设备状态
    /// </summary>
    public DeviceStatusEnum CurrentStatus => _statusSubject.Value;

    protected BaseDevice(
        string id, 
        string name, 
        DeviceTypeEnum type,
        IDeviceConnectionFactory connectionFactory,
        bool isMock = true)
    {
        Id = id;
        Name = name;
        Type = type;
        // TODO: 根据isMock参数选择连接类型
        var t = isMock ? ConnectionTypeEnum.Mock : ConnectionTypeEnum.Modbus;
        _connection = connectionFactory.CreateConnection("XXXXXX", t);
        // 监听连接状态变化，自动更新设备状态
        _connection.ConnectionStateStream
            .DistinctUntilChanged()
            .Subscribe(isConnected =>
            {
                if (!isConnected && _statusSubject.Value == DeviceStatusEnum.Connected)
                {
                    _statusSubject.OnNext(DeviceStatusEnum.Disconnected);
                }
            });
    }
    
    public virtual async Task<bool> ConnectAsync()
    {
        try
        {
            _statusSubject.OnNext(DeviceStatusEnum.Connecting);
            var result = await _connection.OpenAsync();
            _statusSubject.OnNext(result ? DeviceStatusEnum.Connected : DeviceStatusEnum.Error);
            return result;
        }
        catch
        {
            _statusSubject.OnNext(DeviceStatusEnum.Error);
            return false;
        }
    }

    public virtual async Task<bool> DisconnectAsync()
    {
        var result = await _connection.CloseAsync();
        _statusSubject.OnNext(DeviceStatusEnum.Disconnected);
        return result;
    }

    public abstract Task<DeviceData> ReadDataAsync();

    public virtual async Task<CommandResponse> SendCommandAsync(DeviceCommand command)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new CommandResponse
        {
            CommandId = command.CommandId,
            DeviceId = Id
        };

        try
        {
            if (_statusSubject.Value != DeviceStatusEnum.Connected)
            {
                throw new InvalidOperationException("Device not connected");
            }
            // 繁忙状态
            _statusSubject.OnNext(DeviceStatusEnum.Busy);

            // 调用具体设备的指令处理
            response = await ProcessCommandAsync(command);
            response.ExecutionTime = stopwatch.Elapsed;

            // 发布到响应流
            _commandResponseSubject.OnNext(response);

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
            response.ExecutionTime = stopwatch.Elapsed;

            _commandResponseSubject.OnNext(response);
            return response;
        }
        finally
        {
            _statusSubject.OnNext(DeviceStatusEnum.Connected);
        }
    }

    /// <summary>
    /// 子类实现具体的指令处理逻辑
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    protected abstract Task<CommandResponse> ProcessCommandAsync(DeviceCommand command);

    public virtual void Dispose()
    {
        _statusSubject.Dispose();
        _commandResponseSubject.Dispose();
        _connection?.Dispose();
    }
}