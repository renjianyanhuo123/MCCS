using System.Reactive.Linq;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Processors;

/// <summary>
/// 数据聚合处理器
/// 支持滑动窗口统计
/// </summary>
public class DataAggregationProcessor : IDataProcessor
{
    private readonly TimeSpan _windowSize;

    public string Name => "Aggregation";

    public DataAggregationProcessor(TimeSpan windowSize)
    {
        if (windowSize <= TimeSpan.Zero)
            throw new ArgumentException("Window size must be positive", nameof(windowSize));

        _windowSize = windowSize;
    }

    public IObservable<ProcessedData> Process(IObservable<RawHardwareData> source)
    {
        return source
            .Buffer(_windowSize)
            .Where(batch => batch.Count > 0) // 确保批次不为空
            .Select(batch => new ProcessedData
            {
                Raw = batch[^1], // 使用最后一个数据点作为代表
                AggregatedData = CalculateAggregatedMetrics(batch),
                Quality = DataQuality.Good,
                Timestamp = batch[^1].Timestamp
            });
    }

    private AggregatedMetrics CalculateAggregatedMetrics(IList<RawHardwareData> batch)
    {
        var forceValues = batch.Select(d => d.Net_FeedLoadN).ToList();

        var mean = forceValues.Average();
        var max = forceValues.Max();
        var min = forceValues.Min();
        var stdDev = CalculateStdDev(forceValues, mean);

        return new AggregatedMetrics
        {
            Count = batch.Count,
            Mean = mean,
            Max = max,
            Min = min,
            StdDev = stdDev
        };
    }

    private double CalculateStdDev(IEnumerable<float> values, double mean)
    {
        var variance = values.Average(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(variance);
    }
}
