namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 数据发布器接口; 用于发布通道数据和系统状态
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
    void PublishChannelData(long channelId, double value);

    /// <summary>
    /// 发布批量通道数据
    /// </summary>
    void PublishChannelDataBatch(IEnumerable<(long channelId, double value)> dataItems);

    ///// <summary>
    ///// 发布系统状态
    ///// </summary>
    // void PublishSystemStatus(SystemStatusPacket status); 
} 
