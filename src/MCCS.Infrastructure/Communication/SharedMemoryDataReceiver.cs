using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Infrastructure.Communication;

/// <summary>
/// 接收器统计信息
/// </summary>
public record ReceiverStatistics
{
    public long TotalPacketsReceived { get; init; }
    public long LostPacketsCount { get; init; }
    public DateTime LastReceivedTime { get; init; }
    public double AverageLatencyMs { get; init; }
    public double PacketsPerSecond { get; init; }
}

/// <summary>
/// 连接状态变化事件参数
/// </summary>
public class ConnectionStateChangedEventArgs : EventArgs
{
    public bool IsConnected { get; init; }
    public DateTime Timestamp { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// 共享内存数据接收器
/// 从共享内存读取数据并提供响应式数据流
/// </summary>
/// <typeparam name="TData">数据类型</typeparam>
public sealed class SharedMemoryDataReceiver<TData> : IDisposable where TData : struct
{
    private readonly string _channelName;
    private readonly int _maxItems;
    private readonly int _pollIntervalMs;

    private SharedMemoryChannel<TData>? _channel;
    private CancellationTokenSource? _cts;
    private Task? _pollTask;

    private readonly Subject<TData> _dataSubject;
    private readonly ConcurrentDictionary<long, Subject<TData>> _filteredSubjects;

    private readonly Stopwatch _rateStopwatch;
    private long _totalPacketsReceived;
    private long _lostPacketsCount;
    private long _lastSequenceNumber;
    private DateTime _lastReceivedTime;
    private long _packetsInLastSecond;
    private DateTime _lastRateCalculation;

    private volatile bool _isRunning;
    private volatile bool _isConnected;

    public bool IsRunning => _isRunning;
    public bool IsConnected => _isConnected;

    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
    public event EventHandler<TData>? DataReceived;

    /// <summary>
    /// 心跳超时时间（毫秒）
    /// </summary>
    public int HeartbeatTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// 创建共享内存数据接收器
    /// </summary>
    /// <param name="channelName">通道名称</param>
    /// <param name="maxItems">最大项数（应与发布者一致）</param>
    /// <param name="pollIntervalMs">轮询间隔（毫秒）</param>
    public SharedMemoryDataReceiver(string channelName, int maxItems = 1000, int pollIntervalMs = 10)
    {
        _channelName = channelName;
        _maxItems = maxItems;
        _pollIntervalMs = pollIntervalMs;

        _dataSubject = new Subject<TData>();
        _filteredSubjects = new ConcurrentDictionary<long, Subject<TData>>();
        _rateStopwatch = new Stopwatch();
        _lastRateCalculation = DateTime.UtcNow;
    }

    /// <summary>
    /// 启动接收服务
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            _channel = new SharedMemoryChannel<TData>(_channelName, _maxItems);
            _isConnected = true;
            OnConnectionStateChanged(true, "Connected to shared memory channel");
        }
        catch (Exception ex)
        {
            _isConnected = false;
            OnConnectionStateChanged(false, $"Failed to connect: {ex.Message}");
            throw;
        }

        _rateStopwatch.Start();
        _isRunning = true;

        _pollTask = Task.Run(() => PollDataAsync(_cts.Token), _cts.Token);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 停止接收服务
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        await _cts?.CancelAsync()!;

        if (_pollTask != null)
        {
            try
            {
                await _pollTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
        }

        _rateStopwatch.Stop();
        _isConnected = false;
        OnConnectionStateChanged(false, "Receiver stopped");
    }

    private async Task PollDataAsync(CancellationToken cancellationToken)
    {
        var lastHeartbeatTime = DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            try
            {
                // 批量读取数据
                var dataItems = _channel?.ReadBatch(100) ?? [];

                if (dataItems.Count > 0)
                {
                    lastHeartbeatTime = DateTime.UtcNow;
                    _lastReceivedTime = DateTime.UtcNow;

                    foreach (var data in dataItems)
                    {
                        ProcessData(data);
                    } 
                    if (!_isConnected)
                    {
                        _isConnected = true;
                        OnConnectionStateChanged(true, "Reconnected");
                    }
                }
                else
                {
                    // 检查心跳超时
                    if ((DateTime.UtcNow - lastHeartbeatTime).TotalMilliseconds > HeartbeatTimeoutMs)
                    {
                        if (_isConnected)
                        {
                            _isConnected = false;
                            OnConnectionStateChanged(false, "Heartbeat timeout");
                        }
                    }
                }

                // 计算速率
                UpdateRate();

                await Task.Delay(_pollIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // 继续轮询
                await Task.Delay(_pollIntervalMs * 10, cancellationToken);
            }
        }
    }

    private void ProcessData(TData data)
    {
        Interlocked.Increment(ref _totalPacketsReceived);
        Interlocked.Increment(ref _packetsInLastSecond); 
        // 发送到主数据流
        _dataSubject.OnNext(data); 
        // 触发事件
        DataReceived?.Invoke(this, data);
    }

    private void UpdateRate()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastRateCalculation).TotalSeconds >= 1)
        {
            Interlocked.Exchange(ref _packetsInLastSecond, 0);
            _lastRateCalculation = now;
        }
    }

    /// <summary>
    /// 获取所有数据的响应式流
    /// </summary>
    public IObservable<TData> GetDataStream() => _dataSubject.AsObservable();

    /// <summary>
    /// 获取过滤后的数据流（根据提供的过滤函数）
    /// </summary>
    public IObservable<TData> GetFilteredDataStream(Func<TData, bool> filter) => _dataSubject.Where(filter);

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public ReceiverStatistics GetStatistics() =>
        new()
        {
            TotalPacketsReceived = _totalPacketsReceived,
            LostPacketsCount = _lostPacketsCount,
            LastReceivedTime = _lastReceivedTime,
            AverageLatencyMs = 0, // TODO: 计算实际延迟
            PacketsPerSecond = _packetsInLastSecond
        };

    /// <summary>
    /// 获取缓冲区状态
    /// </summary>
    public (int count, int capacity) GetBufferStatus() => _channel?.GetBufferStatus() ?? (0, 0);

    private void OnConnectionStateChanged(bool isConnected, string? reason)
    {
        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
        {
            IsConnected = isConnected,
            Timestamp = DateTime.UtcNow,
            Reason = reason
        });
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            Task.Run(async () =>
            {
                await StopAsync();
            });
        }

        _dataSubject.Dispose();

        foreach (var subject in _filteredSubjects.Values)
        {
            subject.Dispose();
        }
        _filteredSubjects.Clear();

        _channel?.Dispose();
        _cts?.Dispose();
    }
}
