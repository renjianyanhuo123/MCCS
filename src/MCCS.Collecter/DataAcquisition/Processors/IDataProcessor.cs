using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Processors;

/// <summary>
/// 数据处理器接口
/// </summary>
public interface IDataProcessor
{
    /// <summary>
    /// 处理器名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 处理数据流
    /// </summary>
    /// <param name="source">原始数据流</param>
    /// <returns>处理后的数据流</returns>
    IObservable<ProcessedData> Process(IObservable<RawHardwareData> source);
}
