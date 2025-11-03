using System.Reactive.Concurrency;
using MCCS.Collecter.DataAcquisition.Backpressure;
using MCCS.Collecter.DataAcquisition.Processors;
using MCCS.Collecter.DataAcquisition.Strategies;
using MCCS.Collecter.HardwareAdapters;

namespace MCCS.Collecter.DataAcquisition;

/// <summary>
/// 数据采集管道构建器
/// 使用流畅接口模式构建管道
/// </summary>
public class DataAcquisitionPipelineBuilder
{
    private IDataAcquisitionStrategy? _strategy;
    private readonly List<IDataProcessor> _processors = new();
    private IBackpressureStrategy? _backpressureStrategy;
    private IScheduler? _scheduler;

    /// <summary>
    /// 设置采集策略
    /// </summary>
    public DataAcquisitionPipelineBuilder WithStrategy(IDataAcquisitionStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        return this;
    }

    /// <summary>
    /// 添加数据处理器
    /// </summary>
    public DataAcquisitionPipelineBuilder AddProcessor(IDataProcessor processor)
    {
        if (processor == null)
            throw new ArgumentNullException(nameof(processor));

        _processors.Add(processor);
        return this;
    }

    /// <summary>
    /// 设置背压策略
    /// </summary>
    public DataAcquisitionPipelineBuilder WithBackpressure(IBackpressureStrategy strategy)
    {
        _backpressureStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        return this;
    }

    /// <summary>
    /// 设置调度器
    /// </summary>
    public DataAcquisitionPipelineBuilder OnScheduler(IScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        return this;
    }

    /// <summary>
    /// 构建管道
    /// </summary>
    /// <param name="adapter">硬件适配器</param>
    /// <returns>数据采集管道</returns>
    public DataAcquisitionPipeline Build(IHardwareAdapter adapter)
    {
        if (adapter == null)
            throw new ArgumentNullException(nameof(adapter));

        if (_strategy == null)
            throw new InvalidOperationException("采集策略未设置，请调用 WithStrategy()");

        // 使用默认调度器
        _scheduler ??= TaskPoolScheduler.Default;

        return new DataAcquisitionPipeline(
            adapter,
            _strategy,
            _processors,
            _backpressureStrategy,
            _scheduler
        );
    }

    /// <summary>
    /// 重置构建器
    /// </summary>
    public DataAcquisitionPipelineBuilder Reset()
    {
        _strategy = null;
        _processors.Clear();
        _backpressureStrategy = null;
        _scheduler = null;
        return this;
    }
}
