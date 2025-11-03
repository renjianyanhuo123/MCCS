namespace MCCS.Infrastructure.TestModels.DataAcquisition;

/// <summary>
/// 原始硬件数据结构
/// </summary>
public record RawHardwareData
{
    /// <summary>
    /// 时间戳 (高精度 Stopwatch)
    /// </summary>
    public long Timestamp { get; init; }

    /// <summary>
    /// AI 采样数据 (6通道)
    /// </summary>
    public float[] Net_AD_N { get; init; } = Array.Empty<float>();

    /// <summary>
    /// SSI 采样数据 (2通道)
    /// </summary>
    public float[] Net_AD_S { get; init; } = Array.Empty<float>();

    /// <summary>
    /// 位置参考值
    /// </summary>
    public float Net_PosVref { get; init; }

    /// <summary>
    /// 位置误差
    /// </summary>
    public float Net_PosE { get; init; }

    /// <summary>
    /// 控制 DA 输出
    /// </summary>
    public float Net_CtrlDA { get; init; }

    /// <summary>
    /// 循环计数
    /// </summary>
    public int Net_CycleCount { get; init; }

    /// <summary>
    /// 系统状态
    /// </summary>
    public int Net_SysState { get; init; }

    /// <summary>
    /// 测试力反馈 (关键数据)
    /// </summary>
    public float Net_FeedLoadN { get; init; }

    /// <summary>
    /// 数字输入值
    /// </summary>
    public int Net_DIVal { get; init; }

    /// <summary>
    /// 数字输出值
    /// </summary>
    public int Net_DOVal { get; init; }

    /// <summary>
    /// 位置参考值 (数字)
    /// </summary>
    public float Net_D_PosVref { get; init; }

    /// <summary>
    /// 保护错误状态
    /// </summary>
    public int Net_PrtErrState { get; init; }

    /// <summary>
    /// 时间计数
    /// </summary>
    public int Net_TimeCnt { get; init; }
}
