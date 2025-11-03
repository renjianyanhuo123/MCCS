using System.Reactive.Linq;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Processors;

/// <summary>
/// 数据验证接口
/// </summary>
public interface IDataValidator
{
    DataQuality Validate(RawHardwareData data);
}

/// <summary>
/// 默认数据验证器
/// </summary>
public class DefaultDataValidator : IDataValidator
{
    public DataQuality Validate(RawHardwareData data)
    {
        // 基本验证规则
        if (data.Net_AD_N.Length == 0 && data.Net_AD_S.Length == 0)
        {
            return DataQuality.Bad;
        }

        // 检查是否有异常值 (如 NaN, Infinity)
        if (float.IsNaN(data.Net_FeedLoadN) || float.IsInfinity(data.Net_FeedLoadN))
        {
            return DataQuality.Bad;
        }

        // 检查时间戳是否有效
        if (data.Timestamp <= 0)
        {
            return DataQuality.Bad;
        }

        // 可以添加更多验证规则...
        // 例如：检查数据范围、检查变化率等

        return DataQuality.Good;
    }
}

/// <summary>
/// 数据验证处理器
/// </summary>
public class DataValidationProcessor : IDataProcessor
{
    private readonly IDataValidator _validator;

    public string Name => "Validation";

    public DataValidationProcessor(IDataValidator? validator = null)
    {
        _validator = validator ?? new DefaultDataValidator();
    }

    public IObservable<ProcessedData> Process(IObservable<RawHardwareData> source)
    {
        return source
            .Select(raw => new ProcessedData
            {
                Raw = raw,
                Quality = _validator.Validate(raw),
                Timestamp = raw.Timestamp
            })
            .Where(data => data.Quality != DataQuality.Bad); // 过滤坏数据
    }
}
