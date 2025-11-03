using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MCCS.Collecter.DataAcquisition.Backpressure;
using MCCS.Collecter.DataAcquisition.Processors;
using MCCS.Collecter.DataAcquisition.Strategies;
using MCCS.Collecter.HardwareAdapters;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition;

/// <summary>
/// 数据采集管道
/// 整合采集策略、处理器链和背压策略
/// </summary>
public class DataAcquisitionPipeline : IDisposable
{
    private readonly IHardwareAdapter _adapter;
    private readonly IDataAcquisitionStrategy _strategy;
    private readonly List<IDataProcessor> _processors;
    private readonly IBackpressureStrategy? _backpressureStrategy;
    private readonly IScheduler _scheduler;
    private readonly CancellationTokenSource _cts = new();
    private IDisposable? _subscription;
    private bool _disposed = false;
    private bool _isRunning = false;

    /// <summary>
    /// 处理后的数据流
    /// </summary>
    public IObservable<ProcessedData> DataStream { get; }

    /// <summary>
    /// 管道统计信息
    /// </summary>
    public PipelineStats Stats { get; private set; } = new();

    internal DataAcquisitionPipeline(
        IHardwareAdapter adapter,
        IDataAcquisitionStrategy strategy,
        List<IDataProcessor> processors,
        IBackpressureStrategy? backpressureStrategy,
        IScheduler scheduler)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _processors = processors ?? new List<IDataProcessor>();
        _backpressureStrategy = backpressureStrategy;
        _scheduler = scheduler ?? TaskPoolScheduler.Default;

        DataStream = BuildPipeline();
    }

    private IObservable<ProcessedData> BuildPipeline()
    {
        // 1. 创建原始数据流
        var rawStream = _strategy.CreateAcquisitionStream(_adapter, _scheduler, _cts.Token);

        // 2. 应用背压策略（在处理之前）
        if (_backpressureStrategy != null)
        {
            rawStream = _backpressureStrategy.Apply(rawStream);
#if DEBUG
            Debug.WriteLine($"应用背压策略: {_backpressureStrategy.Name}");
#endif
        }

        // 3. 应用处理器链
        IObservable<ProcessedData>? processedStream = null;

        foreach (var processor in _processors)
        {
            if (processedStream == null)
            {
                // 第一个处理器
                processedStream = processor.Process(rawStream);
            }
            else
            {
                // 后续处理器：将 ProcessedData 转换回 RawHardwareData
                var nextRawStream = processedStream.Select(pd => pd.Raw);
                processedStream = processor.Process(nextRawStream);
            }

#if DEBUG
            Debug.WriteLine($"添加处理器: {processor.Name}");
#endif
        }

        // 如果没有处理器，创建一个简单的 ProcessedData 流
        if (processedStream == null)
        {
            processedStream = rawStream.Select(raw => new ProcessedData
            {
                Raw = raw,
                Quality = DataQuality.Good,
                Timestamp = raw.Timestamp
            });
        }

        // 4. 添加统计和错误处理
        return processedStream
            .Do(_ => IncrementProcessedCount())
            .Catch<ProcessedData, Exception>(ex =>
            {
#if DEBUG
                Debug.WriteLine($"管道错误: {ex.Message}");
#endif
                IncrementErrorCount();
                return Observable.Empty<ProcessedData>(); // 继续运行
            })
            .Publish()  // 多播
            .RefCount(); // 自动连接/断开
    }

    /// <summary>
    /// 启动管道
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            Debug.WriteLine("管道已在运行中");
            return;
        }

        _isRunning = true;
        Stats = new PipelineStats { StartTime = DateTime.Now };

        // 订阅数据流以启动管道
        _subscription = DataStream.Subscribe(
            data =>
            {
                // 数据已通过流发布
            },
            ex =>
            {
                Debug.WriteLine($"管道流错误: {ex.Message}");
                _isRunning = false;
            },
            () =>
            {
                Debug.WriteLine("管道流完成");
                _isRunning = false;
            }
        );

#if DEBUG
        Debug.WriteLine($"数据采集管道已启动 - 策略: {_strategy.Name}, 处理器: {_processors.Count}");
#endif
    }

    /// <summary>
    /// 停止管道
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _cts.Cancel();
        _subscription?.Dispose();
        _isRunning = false;

        Stats.EndTime = DateTime.Now;

#if DEBUG
        Debug.WriteLine($"数据采集管道已停止 - 处理: {Stats.ProcessedCount} 条, 错误: {Stats.ErrorCount} 次");
#endif
    }

    /// <summary>
    /// 管道是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;

    private void IncrementProcessedCount()
    {
        Stats = Stats with { ProcessedCount = Stats.ProcessedCount + 1 };
    }

    private void IncrementErrorCount()
    {
        Stats = Stats with { ErrorCount = Stats.ErrorCount + 1 };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _cts?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// 管道统计信息
/// </summary>
public record PipelineStats
{
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public long ProcessedCount { get; init; }
    public long ErrorCount { get; init; }

    public TimeSpan? RunningTime =>
        StartTime.HasValue && EndTime.HasValue
            ? EndTime.Value - StartTime.Value
            : null;
}
