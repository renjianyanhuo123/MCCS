using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Infrastructure.Communication;

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
    private readonly Subject<TData> _dataSubject = new();

    private volatile bool _isRunning;
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 创建共享内存数据接收器
    /// </summary>
    /// <param name="channelName">通道名称</param>
    /// <param name="maxItems">最大项数（应与发布者一致）</param>
    /// <param name="pollIntervalMs">轮询间隔（毫秒）</param>
    public SharedMemoryDataReceiver(
        string channelName,
        int maxItems = 1000,
        int pollIntervalMs = 10)
    {
        _channelName = channelName;
        _maxItems = maxItems;
        _pollIntervalMs = pollIntervalMs;
    }

    /// <summary>
    /// 启动接收服务
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _channel = new SharedMemoryChannel<TData>(_channelName, _maxItems);

        _isRunning = true;
        _pollTask = Task.Run(() => PollDataAsync(_cts.Token), _cts.Token);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 停止接收服务
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning) return;

        _isRunning = false;

        if (_cts != null)
        {
            await _cts.CancelAsync();
        }

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
    }

    private async Task PollDataAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            try
            {
                var dataItems = _channel?.ReadBatch(100) ?? [];

                if (dataItems.Count > 0)
                {
                    foreach (var data in dataItems)
                    {
                        _dataSubject.OnNext(data);
                    }
                }

                await Task.Delay(_pollIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // 出错时延长等待时间后继续
                await Task.Delay(_pollIntervalMs * 10, cancellationToken);
            }
        }
    }

    /// <summary>
    /// 获取数据的响应式流
    /// </summary>
    public IObservable<TData> GetDataStream() => _dataSubject.AsObservable();

    /// <summary>
    /// 获取过滤后的数据流
    /// </summary>
    public IObservable<TData> GetFilteredDataStream(Func<TData, bool> filter) => _dataSubject.Where(filter);

    /// <summary>
    /// 获取缓冲区状态
    /// </summary>
    public (int count, int capacity) GetBufferStatus() => _channel?.GetBufferStatus() ?? (0, 0);

    public void Dispose()
    {
        if (_isRunning)
        {
            StopAsync().GetAwaiter().GetResult();
        }

        _dataSubject.Dispose();
        _channel?.Dispose();
        _cts?.Dispose();
    }
}
