using MCCS.Infrastructure.TestModels.Commands;

namespace MCCS.Infrastructure.TestModels.DataAcquisition;

/// <summary>
/// 处理后的数据
/// </summary>
public record ProcessedData
{
    /// <summary>
    /// 原始数据引用
    /// </summary>
    public RawHardwareData Raw { get; init; } = null!;

    /// <summary>
    /// 转换后的采集模型
    /// </summary>
    public BatchCollectItemModel? CollectModel { get; init; }

    /// <summary>
    /// 聚合统计数据
    /// </summary>
    public AggregatedMetrics? AggregatedData { get; init; }

    /// <summary>
    /// 数据质量
    /// </summary>
    public DataQuality Quality { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; init; }
}

/// <summary>
/// 聚合统计指标
/// </summary>
public record AggregatedMetrics
{
    /// <summary>
    /// 样本数量
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// 平均值
    /// </summary>
    public double Mean { get; init; }

    /// <summary>
    /// 最大值
    /// </summary>
    public float Max { get; init; }

    /// <summary>
    /// 最小值
    /// </summary>
    public float Min { get; init; }

    /// <summary>
    /// 标准差
    /// </summary>
    public double StdDev { get; init; }
}

/// <summary>
/// 数据质量枚举
/// </summary>
public enum DataQuality
{
    /// <summary>
    /// 良好
    /// </summary>
    Good = 0,

    /// <summary>
    /// 不确定
    /// </summary>
    Uncertain = 1,

    /// <summary>
    /// 损坏
    /// </summary>
    Bad = 2
}
