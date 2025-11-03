namespace MCCS.Collecter.DataAcquisition.Backpressure;

/// <summary>
/// 背压策略接口
/// </summary>
public interface IBackpressureStrategy
{
    /// <summary>
    /// 策略名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 应用背压策略
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="source">源数据流</param>
    /// <returns>应用策略后的数据流</returns>
    IObservable<T> Apply<T>(IObservable<T> source);
}
