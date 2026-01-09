namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 数据接收器接口 - 用于从共享内存接收数据
/// </summary>
public interface IDataReceiver : IDisposable
{
    /// <summary>
    /// 是否正在运行
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 是否连接正常（基于心跳检测）
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 启动接收服务
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止接收服务
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定通道的数据流
    /// </summary>
    IObservable<ChannelDataPacket> GetChannelDataStream(long channelId);

    /// <summary>
    /// 获取所有通道的数据流
    /// </summary>
    IObservable<ChannelDataPacket> GetAllChannelDataStream();

    /// <summary>
    /// 获取系统状态流
    /// </summary>
    IObservable<SystemStatusPacket> GetSystemStatusStream();

    /// <summary>
    /// 获取接收统计信息
    /// </summary>
    ReceiverStatistics GetStatistics();

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// 数据接收事件
    /// </summary>
    event EventHandler<ChannelDataReceivedEventArgs>? DataReceived;
}

/// <summary>
/// 接收器统计信息
/// </summary>
public record ReceiverStatistics
{
    /// <summary>
    /// 总接收数据包数
    /// </summary>
    public long TotalPacketsReceived { get; init; }

    /// <summary>
    /// 丢失数据包数（通过序列号检测）
    /// </summary>
    public long LostPacketsCount { get; init; }

    /// <summary>
    /// 最后接收时间
    /// </summary>
    public DateTime LastReceivedTime { get; init; }

    /// <summary>
    /// 平均接收延迟（毫秒）
    /// </summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>
    /// 当前数据接收速率（包/秒）
    /// </summary>
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
/// 通道数据接收事件参数
/// </summary>
public class ChannelDataReceivedEventArgs : EventArgs
{
    public required ChannelDataPacket Data { get; init; }
}
