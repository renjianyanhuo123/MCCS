using MCCS.Infrastructure.Communication;

namespace MCCS.Infrastructure.Services;

/// <summary>
/// 通道数据变化事件参数
/// </summary>
public class ChannelDataChangedEventArgs : EventArgs
{
    public long ChannelId { get; init; }
    public double Value { get; init; }
    public long SequenceIndex { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// 通道数据服务接口
/// 提供从共享内存读取通道数据的统一访问接口
/// </summary>
public interface IChannelDataService : IDisposable
{
    /// <summary>
    /// 服务是否正在运行
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 是否已连接到共享内存
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// 启动数据接收服务
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止数据接收服务
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有通道数据的响应式流
    /// </summary>
    IObservable<ChannelDataItem> GetAllChannelDataStream();

    /// <summary>
    /// 获取指定通道的数据流
    /// </summary>
    IObservable<ChannelDataItem> GetChannelDataStream(long channelId);

    /// <summary>
    /// 获取多个通道的数据流
    /// </summary>
    IObservable<ChannelDataItem> GetChannelDataStream(IEnumerable<long> channelIds);

    /// <summary>
    /// 获取指定通道的当前值
    /// </summary>
    double? GetCurrentValue(long channelId);

    /// <summary>
    /// 获取多个通道的当前值
    /// </summary>
    Dictionary<long, double> GetCurrentValues(IEnumerable<long> channelIds);

    /// <summary>
    /// 订阅指定通道的数据变化
    /// </summary>
    IDisposable Subscribe(long channelId, Action<ChannelDataItem> onData);

    /// <summary>
    /// 订阅多个通道的数据变化
    /// </summary>
    IDisposable Subscribe(IEnumerable<long> channelIds, Action<ChannelDataItem> onData);
}
