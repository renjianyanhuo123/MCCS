namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 共享内存通信常量配置
/// </summary>
public static class SharedMemoryConstants
{
    /// <summary>
    /// 数据通道名称前缀
    /// </summary>
    public const string DataChannelPrefix = "MCCS_Data_";

    /// <summary>
    /// 状态通道名称
    /// </summary>
    public const string StatusChannelName = "MCCS_Status";

    /// <summary>
    /// 命令通道名称
    /// </summary>
    public const string CommandChannelName = "MCCS_Command";

    /// <summary>
    /// 默认数据通道最大项数
    /// </summary>
    public const int DefaultDataChannelMaxItems = 500;

    /// <summary>
    /// 默认状态通道最大项数
    /// </summary>
    public const int DefaultStatusChannelMaxItems = 500;

    /// <summary>
    /// 默认命令通道最大项数
    /// </summary>
    public const int DefaultCommandChannelMaxItems = 500;

    /// <summary>
    /// 数据包最大尺寸（字节）
    /// </summary>
    public const int MaxPacketSize = 8192;

    /// <summary>
    /// 批量数据最大项数
    /// </summary>
    public const int MaxBatchItemCount = 100;

    /// <summary>
    /// 心跳间隔（毫秒）
    /// </summary>
    public const int HeartbeatIntervalMs = 1000;

    /// <summary>
    /// 心跳超时时间（毫秒）
    /// </summary>
    public const int HeartbeatTimeoutMs = 5000;

    /// <summary>
    /// 数据读取轮询间隔（毫秒）
    /// </summary>
    public const int DataPollIntervalMs = 10;
}
