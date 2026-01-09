using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MCCS.Infrastructure.Communication;

namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 站点数据接收器 - 用于主UI应用从共享内存接收通道数据
/// 提供类型安全的数据流访问
/// </summary>
public sealed class StationDataReceiver : IDataReceiver
{
    private readonly SharedMemoryDataReceiver<ChannelDataPacket> _dataReceiver;
    private readonly SharedMemoryDataReceiver<SystemStatusPacket> _statusReceiver;

    private readonly Subject<ChannelDataPacket> _channelDataSubject;
    private readonly Subject<SystemStatusPacket> _systemStatusSubject;
    private readonly ConcurrentDictionary<long, Subject<ChannelDataPacket>> _channelSubjects;
    private readonly ConcurrentDictionary<long, long> _lastSequenceNumbers;

    private CancellationTokenSource? _cts;
    private volatile bool _isRunning;
    private volatile bool _isConnected;

    private long _totalPacketsReceived;
    private long _lostPacketsCount;
    private DateTime _lastReceivedTime;

    /// <summary>
    /// 数据通道名称
    /// </summary>
    public const string DataChannelName = "MCCS_ChannelData";

    /// <summary>
    /// 状态通道名称
    /// </summary>
    public const string StatusChannelName = "MCCS_SystemStatus";

    public bool IsRunning => _isRunning;
    public bool IsConnected => _isConnected;

    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
    public event EventHandler<ChannelDataReceivedEventArgs>? DataReceived;

    public StationDataReceiver(int pollIntervalMs = 10)
    {
        _dataReceiver = new SharedMemoryDataReceiver<ChannelDataPacket>(
            DataChannelName,
            SharedMemoryConstants.DefaultDataChannelMaxItems,
            pollIntervalMs);

        _statusReceiver = new SharedMemoryDataReceiver<SystemStatusPacket>(
            StatusChannelName,
            SharedMemoryConstants.DefaultStatusChannelMaxItems,
            pollIntervalMs * 10); // 状态通道轮询频率较低

        _channelDataSubject = new Subject<ChannelDataPacket>();
        _systemStatusSubject = new Subject<SystemStatusPacket>();
        _channelSubjects = new ConcurrentDictionary<long, Subject<ChannelDataPacket>>();
        _lastSequenceNumbers = new ConcurrentDictionary<long, long>();

        // 订阅底层接收器事件
        _dataReceiver.DataReceived += OnDataReceived;
        _dataReceiver.ConnectionStateChanged += OnDataConnectionStateChanged;
        _statusReceiver.DataReceived += OnStatusReceived;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        await _dataReceiver.StartAsync(_cts.Token);
        await _statusReceiver.StartAsync(_cts.Token);

        _isRunning = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cts?.Cancel();

        await _dataReceiver.StopAsync(cancellationToken);
        await _statusReceiver.StopAsync(cancellationToken);
    }

    private void OnDataReceived(object? sender, ChannelDataPacket data)
    {
        Interlocked.Increment(ref _totalPacketsReceived);
        _lastReceivedTime = DateTime.UtcNow;

        // 检查序列号丢失
        var lastSeq = _lastSequenceNumbers.AddOrUpdate(
            data.ChannelId,
            data.SequenceNumber,
            (_, old) =>
            {
                if (data.SequenceNumber > old + 1)
                {
                    Interlocked.Add(ref _lostPacketsCount, data.SequenceNumber - old - 1);
                }
                return data.SequenceNumber;
            });

        // 发送到主数据流
        _channelDataSubject.OnNext(data);

        // 发送到通道特定数据流
        if (_channelSubjects.TryGetValue(data.ChannelId, out var channelSubject))
        {
            channelSubject.OnNext(data);
        }

        // 触发事件
        DataReceived?.Invoke(this, new ChannelDataReceivedEventArgs { Data = data });
    }

    private void OnStatusReceived(object? sender, SystemStatusPacket status)
    {
        _systemStatusSubject.OnNext(status);

        // 心跳消息表示连接正常
        if (status.Type == MessageType.Heartbeat)
        {
            if (!_isConnected)
            {
                _isConnected = true;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                {
                    IsConnected = true,
                    Timestamp = DateTime.UtcNow,
                    Reason = "Heartbeat received"
                });
            }
        }
    }

    private void OnDataConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        _isConnected = e.IsConnected;
        ConnectionStateChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 获取指定通道的数据流
    /// </summary>
    public IObservable<ChannelDataPacket> GetChannelDataStream(long channelId)
    {
        var subject = _channelSubjects.GetOrAdd(channelId, _ => new Subject<ChannelDataPacket>());

        // 同时从主流中过滤（确保能获取到历史数据）
        return _channelDataSubject
            .Where(d => d.ChannelId == channelId)
            .Merge(subject.AsObservable())
            .DistinctUntilChanged(d => d.SequenceNumber);
    }

    /// <summary>
    /// 获取所有通道的数据流
    /// </summary>
    public IObservable<ChannelDataPacket> GetAllChannelDataStream()
    {
        return _channelDataSubject.AsObservable();
    }

    /// <summary>
    /// 获取系统状态流
    /// </summary>
    public IObservable<SystemStatusPacket> GetSystemStatusStream()
    {
        return _systemStatusSubject.AsObservable();
    }

    /// <summary>
    /// 获取指定通道的数据值流（只返回值，简化使用）
    /// </summary>
    public IObservable<double> GetChannelValueStream(long channelId)
    {
        return GetChannelDataStream(channelId).Select(d => d.Value);
    }

    /// <summary>
    /// 获取多个通道的最新值组合
    /// </summary>
    public IObservable<Dictionary<long, double>> GetCombinedChannelValues(params long[] channelIds)
    {
        if (channelIds.Length == 0)
            return Observable.Empty<Dictionary<long, double>>();

        var streams = channelIds.Select(id =>
            GetChannelDataStream(id).Select(d => (d.ChannelId, d.Value)));

        return streams.Merge()
            .Scan(new Dictionary<long, double>(), (dict, item) =>
            {
                var newDict = new Dictionary<long, double>(dict)
                {
                    [item.ChannelId] = item.Value
                };
                return newDict;
            })
            .Where(dict => dict.Count == channelIds.Length);
    }

    public ReceiverStatistics GetStatistics()
    {
        return new ReceiverStatistics
        {
            TotalPacketsReceived = _totalPacketsReceived,
            LostPacketsCount = _lostPacketsCount,
            LastReceivedTime = _lastReceivedTime,
            AverageLatencyMs = 0,
            PacketsPerSecond = _dataReceiver.GetStatistics().PacketsPerSecond
        };
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            StopAsync().GetAwaiter().GetResult();
        }

        _dataReceiver.DataReceived -= OnDataReceived;
        _dataReceiver.ConnectionStateChanged -= OnDataConnectionStateChanged;
        _statusReceiver.DataReceived -= OnStatusReceived;

        _channelDataSubject.Dispose();
        _systemStatusSubject.Dispose();

        foreach (var subject in _channelSubjects.Values)
        {
            subject.Dispose();
        }
        _channelSubjects.Clear();

        _dataReceiver.Dispose();
        _statusReceiver.Dispose();
        _cts?.Dispose();
    }
}
