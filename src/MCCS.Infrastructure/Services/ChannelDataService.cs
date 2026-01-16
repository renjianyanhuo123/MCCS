using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MCCS.Infrastructure.Communication;

namespace MCCS.Infrastructure.Services;

/// <summary>
/// 通道数据服务实现
/// 封装共享内存数据接收，提供响应式数据流和便捷的订阅接口
/// </summary>
public sealed class ChannelDataService : IChannelDataService
{
    private readonly SharedMemoryDataReceiver<ChannelDataItem> _receiver;
    private readonly ConcurrentDictionary<long, double> _currentValues;
    private readonly ConcurrentDictionary<long, long> _sequenceIndices;
    private readonly Subject<ChannelDataItem> _allDataSubject;
    private readonly CompositeDisposable _subscriptions;

    private volatile bool _isRunning;
    private IDisposable? _dataSubscription;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public bool IsConnected => _receiver.IsConnected;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// 创建通道数据服务实例
    /// </summary>
    /// <param name="channelName">共享内存通道名称，默认为 MCCS_ChannelData</param>
    /// <param name="maxItems">最大缓冲项数</param>
    /// <param name="pollIntervalMs">轮询间隔（毫秒）</param>
    public ChannelDataService(
        string? channelName = null,
        int maxItems = 500,
        int pollIntervalMs = 10)
    {
        var name = channelName ?? SharedMemoryConstants.ChannelDataName;
        _receiver = new SharedMemoryDataReceiver<ChannelDataItem>(name, maxItems, pollIntervalMs);
        _receiver.ConnectionStateChanged += OnReceiverConnectionStateChanged;

        _currentValues = new ConcurrentDictionary<long, double>();
        _sequenceIndices = new ConcurrentDictionary<long, long>();
        _allDataSubject = new Subject<ChannelDataItem>();
        _subscriptions = new CompositeDisposable();
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        await _receiver.StartAsync(cancellationToken);

        // 订阅接收器的数据流
        _dataSubscription = _receiver.GetDataStream()
            .Subscribe(OnDataReceived);

        _isRunning = true;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _dataSubscription?.Dispose();
        _dataSubscription = null;

        await _receiver.StopAsync(cancellationToken);
    }

    private void OnDataReceived(ChannelDataItem data)
    {
        // 更新当前值缓存
        _currentValues.AddOrUpdate(data.ChannelId, data.Value, (_, _) => data.Value);
        _sequenceIndices.AddOrUpdate(data.ChannelId, data.SequenceIndex, (_, _) => data.SequenceIndex);

        // 发布到主数据流
        _allDataSubject.OnNext(data);
    }

    private void OnReceiverConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        ConnectionStateChanged?.Invoke(this, e);
    }

    /// <inheritdoc />
    public IObservable<ChannelDataItem> GetAllChannelDataStream()
    {
        return _allDataSubject.AsObservable();
    }

    /// <inheritdoc />
    public IObservable<ChannelDataItem> GetChannelDataStream(long channelId)
    {
        return _allDataSubject
            .Where(data => data.ChannelId == channelId)
            .AsObservable();
    }

    /// <inheritdoc />
    public IObservable<ChannelDataItem> GetChannelDataStream(IEnumerable<long> channelIds)
    {
        var channelSet = new HashSet<long>(channelIds);
        return _allDataSubject
            .Where(data => channelSet.Contains(data.ChannelId))
            .AsObservable();
    }

    /// <inheritdoc />
    public double? GetCurrentValue(long channelId)
    {
        return _currentValues.TryGetValue(channelId, out var value) ? value : null;
    }

    /// <inheritdoc />
    public Dictionary<long, double> GetCurrentValues(IEnumerable<long> channelIds)
    {
        var result = new Dictionary<long, double>();
        foreach (var channelId in channelIds)
        {
            if (_currentValues.TryGetValue(channelId, out var value))
            {
                result[channelId] = value;
            }
        }
        return result;
    }

    /// <inheritdoc />
    public IDisposable Subscribe(long channelId, Action<ChannelDataItem> onDataReceived)
    {
        return GetChannelDataStream(channelId).Subscribe(onDataReceived);
    }

    /// <inheritdoc />
    public IDisposable Subscribe(IEnumerable<long> channelIds, Action<ChannelDataItem> onDataReceived)
    {
        return GetChannelDataStream(channelIds).Subscribe(onDataReceived);
    }

    /// <inheritdoc />
    public ReceiverStatistics GetStatistics()
    {
        return _receiver.GetStatistics();
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            StopAsync().GetAwaiter().GetResult();
        }

        _dataSubscription?.Dispose();
        _subscriptions.Dispose();
        _allDataSubject.Dispose();
        _receiver.ConnectionStateChanged -= OnReceiverConnectionStateChanged;
        _receiver.Dispose();
    }
}
