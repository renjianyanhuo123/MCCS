using System.Reactive.Linq;
using MCCS.Infrastructure.Communication;

namespace MCCS.Infrastructure.Services;

/// <summary>
/// 通道数据流扩展方法
/// 提供常用的数据流操作
/// </summary>
public static class ChannelDataExtensions
{
    /// <summary>
    /// 对数据流进行采样（取指定时间间隔内的最后一个值）
    /// 适用于仪表盘等不需要高频更新的场景
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="interval">采样间隔</param>
    //public static IObservable<ChannelDataItem> Sample(
    //    this IObservable<ChannelDataItem> source,
    //    TimeSpan interval) =>
    //    source.Sample(interval);

    /// <summary>
    /// 对数据流进行节流（在指定时间间隔内只取第一个值）
    /// 适用于控制更新频率的场景
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="interval">节流间隔</param>
    //public static IObservable<ChannelDataItem> Throttle(
    //    this IObservable<ChannelDataItem> source,
    //    TimeSpan interval)
    //{
    //    return source.Throttle(interval);
    //}

    /// <summary>
    /// 按时间窗口缓冲数据
    /// 适用于图表等需要批量数据的场景
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="bufferTimeSpan">缓冲时间</param>
    //public static IObservable<IList<ChannelDataItem>> Buffer(
    //    this IObservable<ChannelDataItem> source,
    //    TimeSpan bufferTimeSpan)
    //{
    //    return source.Buffer(bufferTimeSpan);
    //}

    /// <summary>
    /// 按数量缓冲数据
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="count">缓冲数量</param>
    //public static IObservable<IList<ChannelDataItem>> Buffer(
    //    this IObservable<ChannelDataItem> source,
    //    int count)
    //{
    //    return source.Buffer(count);
    //}

    /// <summary>
    /// 只获取值
    /// </summary>
    /// <param name="source">数据源</param>
    //public static IObservable<double> SelectValue(this IObservable<ChannelDataItem> source)
    //{
    //    return source.Select(data => data.Value);
    //}

    /// <summary>
    /// 当值发生变化时才发出
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="tolerance">容差值，默认为0（精确比较）</param>
    //public static IObservable<ChannelDataItem> DistinctUntilValueChanged(
    //    this IObservable<ChannelDataItem> source,
    //    double tolerance = 0)
    //{
    //    if (tolerance <= 0)
    //    {
    //        return source.DistinctUntilChanged(data => data.Value);
    //    }

    //    return source
    //        .Scan((prev: default(ChannelDataItem?), current: default(ChannelDataItem?)),
    //            (acc, data) => (acc.current, data))
    //        .Where(pair => pair.prev == null ||
    //                       Math.Abs(pair.current!.Value.Value - pair.prev.Value.Value) > tolerance)
    //        .Select(pair => pair.current!.Value);
    //}

    /// <summary>
    /// 添加时间戳
    /// </summary>
    /// <param name="source">数据源</param>
    //public static IObservable<(ChannelDataItem Data, DateTime Timestamp)> WithTimestamp(
    //    this IObservable<ChannelDataItem> source)
    //{
    //    return source.Select(data => (data, DateTime.Now));
    //}

    /// <summary>
    /// 计算变化率（每秒变化量）
    /// </summary>
    /// <param name="source">数据源</param>
    //public static IObservable<double> CalculateRate(this IObservable<ChannelDataItem> source)
    //{
    //    return source
    //        .Timestamp()
    //        .Buffer(2, 1)
    //        .Where(buffer => buffer.Count == 2)
    //        .Select(buffer =>
    //        {
    //            var prev = buffer[0];
    //            var curr = buffer[1];
    //            var timeDelta = (curr.Timestamp - prev.Timestamp).TotalSeconds;
    //            return timeDelta > 0 ? (curr.Value.Value - prev.Value.Value) / timeDelta : 0;
    //        });
    //}

    /// <summary>
    /// 计算滑动平均值
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="windowSize">窗口大小</param>
    //public static IObservable<double> MovingAverage(
    //    this IObservable<ChannelDataItem> source,
    //    int windowSize)
    //{
    //    return source
    //        .Buffer(windowSize, 1)
    //        .Where(buffer => buffer.Count == windowSize)
    //        .Select(buffer => buffer.Average(data => data.Value));
    //}

    /// <summary>
    /// 检测值是否在指定范围内
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    //public static IObservable<bool> IsInRange(
    //    this IObservable<ChannelDataItem> source,
    //    double min,
    //    double max)
    //{
    //    return source.Select(data => data.Value >= min && data.Value <= max);
    //}

    /// <summary>
    /// 当值超出范围时发出警报
    /// </summary>
    /// <param name="source">数据源</param>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    //public static IObservable<ChannelDataItem> WhenOutOfRange(
    //    this IObservable<ChannelDataItem> source,
    //    double min,
    //    double max)
    //{
    //    return source.Where(data => data.Value < min || data.Value > max);
    //}
}
