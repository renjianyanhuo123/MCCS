namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 数据发布器接口 - 用于向共享内存发布数据
/// </summary>
public interface IDataPublisher : IDisposable
{
    /// <summary>
    /// 是否正在运行
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 启动发布服务
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止发布服务
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布单个通道数据
    /// </summary>
    void PublishChannelData(long channelId, double value, DataQuality quality = DataQuality.Good);

    /// <summary>
    /// 发布批量通道数据
    /// </summary>
    void PublishChannelDataBatch(IEnumerable<(long channelId, double value, DataQuality quality)> dataItems);

    /// <summary>
    /// 发布系统状态
    /// </summary>
    void PublishSystemStatus(SystemStatusPacket status);

    /// <summary>
    /// 获取发布统计信息
    /// </summary>
    PublisherStatistics GetStatistics();
}

/// <summary>
/// 发布器统计信息
/// </summary>
public record PublisherStatistics
{
    /// <summary>
    /// 总发布数据包数
    /// </summary>
    public long TotalPacketsPublished { get; init; }

    /// <summary>
    /// 发布失败次数
    /// </summary>
    public long FailedPublishCount { get; init; }

    /// <summary>
    /// 当前队列大小
    /// </summary>
    public int CurrentQueueSize { get; init; }

    /// <summary>
    /// 平均发布延迟（毫秒）
    /// </summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>
    /// 运行时长
    /// </summary>
    public TimeSpan Uptime { get; init; }
}
