using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<long, double> _currentValues = new();
    private readonly Subject<ChannelDataItem> _dataSubject = new();

    private IDisposable? _subscription;
    private volatile bool _isRunning;

    public bool IsRunning => _isRunning;
    public bool IsConnected => _receiver.IsConnected;

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
        _receiver.ConnectionStateChanged += (_, e) => ConnectionStateChanged?.Invoke(this, e);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return;

        await _receiver.StartAsync(cancellationToken);

        _subscription = _receiver.GetDataStream().Subscribe(OnDataReceived);
        _isRunning = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning) return;

        _isRunning = false;
        _subscription?.Dispose();
        _subscription = null;

        await _receiver.StopAsync(cancellationToken);
    }

    private void OnDataReceived(ChannelDataItem data)
    {
        _currentValues[data.ChannelId] = data.Value;
        _dataSubject.OnNext(data);
    }

    public IObservable<ChannelDataItem> GetAllChannelDataStream() => _dataSubject.AsObservable();

    public IObservable<ChannelDataItem> GetChannelDataStream(long channelId) =>
        _dataSubject.Where(d => d.ChannelId == channelId);

    public IObservable<ChannelDataItem> GetChannelDataStream(IEnumerable<long> channelIds)
    {
        var idSet = new HashSet<long>(channelIds);
        return _dataSubject.Where(d => idSet.Contains(d.ChannelId));
    }

    public double? GetCurrentValue(long channelId) =>
        _currentValues.TryGetValue(channelId, out var value) ? value : null;

    public Dictionary<long, double> GetCurrentValues(IEnumerable<long> channelIds)
    {
        var result = new Dictionary<long, double>();
        foreach (var id in channelIds)
        {
            if (_currentValues.TryGetValue(id, out var value))
            {
                result[id] = value;
            }
        }
        return result;
    }

    public IDisposable Subscribe(long channelId, Action<ChannelDataItem> onData) =>
        GetChannelDataStream(channelId).Subscribe(onData);

    public IDisposable Subscribe(IEnumerable<long> channelIds, Action<ChannelDataItem> onData) =>
        GetChannelDataStream(channelIds).Subscribe(onData);

    public void Dispose()
    {
        if (_isRunning)
        {
            StopAsync().GetAwaiter().GetResult();
        }

        _subscription?.Dispose();
        _dataSubject.Dispose();
        _receiver.Dispose();
    }
}
