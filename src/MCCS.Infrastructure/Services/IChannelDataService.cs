using System.Reactive;
using MCCS.Infrastructure.Communication;

namespace MCCS.Infrastructure.Services;

/// <summary>
/// 通道数据变化事件参数
/// </summary>
public class ChannelDataChangedEventArgs : EventArgs
{
    /// <summary>
    /// 通道ID
    /// </summary>
    public long ChannelId { get; init; }

    /// <summary>
    /// 当前值
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// 序列索引
    /// </summary>
    public long SequenceIndex { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
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
    /// <param name="channelId">通道ID</param>
    IObservable<ChannelDataItem> GetChannelDataStream(long channelId);

    /// <summary>
    /// 获取多个通道的数据流
    /// </summary>
    /// <param name="channelIds">通道ID列表</param>
    IObservable<ChannelDataItem> GetChannelDataStream(IEnumerable<long> channelIds);

    /// <summary>
    /// 获取指定通道的当前值
    /// </summary>
    /// <param name="channelId">通道ID</param>
    /// <returns>当前值，如果通道不存在则返回null</returns>
    double? GetCurrentValue(long channelId);

    /// <summary>
    /// 获取多个通道的当前值
    /// </summary>
    /// <param name="channelIds">通道ID列表</param>
    /// <returns>通道ID到值的字典</returns>
    Dictionary<long, double> GetCurrentValues(IEnumerable<long> channelIds);

    /// <summary>
    /// 订阅指定通道的数据变化
    /// </summary>
    /// <param name="channelId">通道ID</param>
    /// <param name="onDataReceived">数据接收回调</param>
    /// <returns>订阅的Disposable，用于取消订阅</returns>
    IDisposable Subscribe(long channelId, Action<ChannelDataItem> onDataReceived);

    /// <summary>
    /// 订阅多个通道的数据变化
    /// </summary>
    /// <param name="channelIds">通道ID列表</param>
    /// <param name="onDataReceived">数据接收回调</param>
    /// <returns>订阅的Disposable，用于取消订阅</returns>
    IDisposable Subscribe(IEnumerable<long> channelIds, Action<ChannelDataItem> onDataReceived);

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// 获取接收器统计信息
    /// </summary>
    ReceiverStatistics GetStatistics();
}
