namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 数据质量枚举
/// </summary>
public enum DataQuality : byte
{
    /// <summary>
    /// 数据良好
    /// </summary>
    Good = 0,

    /// <summary>
    /// 数据不确定
    /// </summary>
    Uncertain = 1,

    /// <summary>
    /// 数据无效
    /// </summary>
    Bad = 2
}
