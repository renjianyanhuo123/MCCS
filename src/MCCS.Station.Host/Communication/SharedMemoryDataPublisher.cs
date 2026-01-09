using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MCCS.Infrastructure.Communication;
using MCCS.Station.Abstractions.Communication;
using MCCS.Station.Core.HardwareDevices;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;

namespace MCCS.Station.Host.Communication;

/// <summary>
/// 共享内存数据发布服务
/// 订阅信号和虚拟通道数据流，并通过共享内存发布给客户端
/// </summary>
public sealed class SharedMemoryDataPublisher : IDataPublisher
{
    private readonly ISignalManager _signalManager;
    private readonly IPseudoChannelManager _pseudoChannelManager;
    private readonly SharedMemoryChannelManager _channelManager;
    private readonly CompositeDisposable _subscriptions;
    private readonly ConcurrentDictionary<long, long> _channelSequenceNumbers;

    private SharedMemoryChannel<ChannelDataPacket>? _dataChannel;
    private SharedMemoryChannel<SystemStatusPacket>? _statusChannel;

    private CancellationTokenSource? _cts;
    private Task? _heartbeatTask;
    private Task? _statusTask;

    private readonly Stopwatch _uptimeStopwatch;
    private long _totalPacketsPublished;
    private long _failedPublishCount;

    private volatile bool _isRunning;

    public bool IsRunning => _isRunning;

    /// <summary>
    /// 数据通道名称
    /// </summary>
    public const string DataChannelName = "MCCS_ChannelData";

    /// <summary>
    /// 状态通道名称
    /// </summary>
    public const string StatusChannelName = "MCCS_SystemStatus";

    public SharedMemoryDataPublisher(
        ISignalManager signalManager,
        IPseudoChannelManager pseudoChannelManager)
    {
        _signalManager = signalManager;
        _pseudoChannelManager = pseudoChannelManager;
        _channelManager = new SharedMemoryChannelManager();
        _subscriptions = new CompositeDisposable();
        _channelSequenceNumbers = new ConcurrentDictionary<long, long>();
        _uptimeStopwatch = new Stopwatch();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 初始化共享内存通道
        _dataChannel = _channelManager.GetOrCreateChannel<ChannelDataPacket>(
            DataChannelName,
            SharedMemoryConstants.DefaultDataChannelMaxItems);

        _statusChannel = _channelManager.GetOrCreateChannel<SystemStatusPacket>(
            StatusChannelName,
            SharedMemoryConstants.DefaultStatusChannelMaxItems);

        // 订阅所有虚拟通道数据
        SubscribeToPseudoChannels();

        // 启动心跳和状态发布任务
        _heartbeatTask = StartHeartbeatAsync(_cts.Token);
        _statusTask = StartStatusPublishAsync(_cts.Token);

        _uptimeStopwatch.Start();
        _isRunning = true;

#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SharedMemoryDataPublisher started");
#endif

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return;

        _isRunning = false;

        _cts?.Cancel();

        // 等待后台任务完成
        if (_heartbeatTask != null)
            await _heartbeatTask.ConfigureAwait(false);
        if (_statusTask != null)
            await _statusTask.ConfigureAwait(false);

        // 清理订阅
        _subscriptions.Clear();

        _uptimeStopwatch.Stop();

#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SharedMemoryDataPublisher stopped");
#endif
    }

    private void SubscribeToPseudoChannels()
    {
        var pseudoChannels = _pseudoChannelManager.GetPseudoChannels();

        foreach (var channel in pseudoChannels)
        {
            var channelId = channel.ChannelId;

            // 订阅虚拟通道数据流
            var subscription = channel.GetPseudoChannelStream()
                .Subscribe(
                    dataPoint => PublishChannelDataInternal(channelId, dataPoint),
                    ex => HandleError(channelId, ex),
                    () => { /* completed */ });

            _subscriptions.Add(subscription);
        }

#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Subscribed to {pseudoChannels.Count()} pseudo channels");
#endif
    }

    private void PublishChannelDataInternal(long channelId, DataPoint<float> dataPoint)
    {
        if (_dataChannel == null || !_isRunning)
            return;

        try
        {
            var sequenceNumber = _channelSequenceNumbers.AddOrUpdate(
                channelId, 1, (_, seq) => seq + 1);

            var packet = new ChannelDataPacket
            {
                Type = MessageType.PseudoChannelData,
                ChannelId = channelId,
                Timestamp = dataPoint.Timestamp,
                Value = dataPoint.Value,
                Quality = (DataQuality)(byte)dataPoint.DataQuality,
                SequenceNumber = sequenceNumber,
                Reserved = 0
            };

            if (_dataChannel.TryWrite(packet, 10))
            {
                Interlocked.Increment(ref _totalPacketsPublished);
            }
            else
            {
                Interlocked.Increment(ref _failedPublishCount);
            }
        }
        catch (Exception)
        {
            Interlocked.Increment(ref _failedPublishCount);
        }
    }

    private void HandleError(long channelId, Exception ex)
    {
#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error in channel {channelId}: {ex.Message}");
#endif
    }

    public void PublishChannelData(long channelId, double value, DataQuality quality = DataQuality.Good)
    {
        if (_dataChannel == null || !_isRunning)
            return;

        var sequenceNumber = _channelSequenceNumbers.AddOrUpdate(
            channelId, 1, (_, seq) => seq + 1);

        var packet = new ChannelDataPacket
        {
            Type = MessageType.PseudoChannelData,
            ChannelId = channelId,
            Timestamp = DateTime.UtcNow.Ticks,
            Value = value,
            Quality = quality,
            SequenceNumber = sequenceNumber,
            Reserved = 0
        };

        if (_dataChannel.TryWrite(packet, 10))
        {
            Interlocked.Increment(ref _totalPacketsPublished);
        }
        else
        {
            Interlocked.Increment(ref _failedPublishCount);
        }
    }

    public void PublishChannelDataBatch(IEnumerable<(long channelId, double value, DataQuality quality)> dataItems)
    {
        if (_dataChannel == null || !_isRunning)
            return;

        var timestamp = DateTime.UtcNow.Ticks;
        var packets = new List<ChannelDataPacket>();

        foreach (var (channelId, value, quality) in dataItems)
        {
            var sequenceNumber = _channelSequenceNumbers.AddOrUpdate(
                channelId, 1, (_, seq) => seq + 1);

            packets.Add(new ChannelDataPacket
            {
                Type = MessageType.PseudoChannelData,
                ChannelId = channelId,
                Timestamp = timestamp,
                Value = value,
                Quality = quality,
                SequenceNumber = sequenceNumber,
                Reserved = 0
            });
        }

        _dataChannel.WriteBatch(packets);
        Interlocked.Add(ref _totalPacketsPublished, packets.Count);
    }

    public void PublishSystemStatus(SystemStatusPacket status)
    {
        if (_statusChannel == null || !_isRunning)
            return;

        status.Type = MessageType.SystemStatus;
        status.Timestamp = DateTime.UtcNow.Ticks;
        status.TotalPacketsSent = _totalPacketsPublished;

        _statusChannel.TryWrite(status, 10);
    }

    private async Task StartHeartbeatAsync(CancellationToken cancellationToken)
    {
        var heartbeatCount = 0L;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(SharedMemoryConstants.HeartbeatIntervalMs, cancellationToken);

                if (_statusChannel != null && _isRunning)
                {
                    var heartbeat = new SystemStatusPacket
                    {
                        Type = MessageType.Heartbeat,
                        Timestamp = DateTime.UtcNow.Ticks,
                        State = SystemState.Running,
                        HeartbeatCount = ++heartbeatCount,
                        TotalPacketsSent = _totalPacketsPublished,
                        ErrorCode = 0
                    };

                    _statusChannel.TryWrite(heartbeat, 10);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // 忽略心跳错误
            }
        }
    }

    private async Task StartStatusPublishAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, cancellationToken); // 每5秒发布一次系统状态

                if (_statusChannel != null && _isRunning)
                {
                    var status = new SystemStatusPacket
                    {
                        Type = MessageType.SystemStatus,
                        Timestamp = DateTime.UtcNow.Ticks,
                        State = SystemState.Running,
                        ActiveControllerCount = 1, // TODO: 从ControllerManager获取
                        ActiveSignalCount = _channelSequenceNumbers.Count,
                        SampleRate = 500,
                        HeartbeatCount = 0,
                        TotalPacketsSent = _totalPacketsPublished,
                        ErrorCode = 0
                    };

                    _statusChannel.TryWrite(status, 10);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // 忽略状态发布错误
            }
        }
    }

    public PublisherStatistics GetStatistics()
    {
        return new PublisherStatistics
        {
            TotalPacketsPublished = _totalPacketsPublished,
            FailedPublishCount = _failedPublishCount,
            CurrentQueueSize = _dataChannel?.GetBufferStatus().count ?? 0,
            AverageLatencyMs = 0, // TODO: 计算实际延迟
            Uptime = _uptimeStopwatch.Elapsed
        };
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            StopAsync().GetAwaiter().GetResult();
        }

        _subscriptions.Dispose();
        _channelManager.Dispose();
        _cts?.Dispose();
    }
}
