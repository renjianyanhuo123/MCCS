using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MCCS.Collecter.HardwareAdapters;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Strategies;

/// <summary>
/// 自适应采集策略（动态调整采样率）
/// </summary>
public class AdaptiveRateStrategy : IDataAcquisitionStrategy
{
    private int _currentSampleRate;
    private readonly int _minSampleRate;
    private readonly int _maxSampleRate;
    private readonly Func<RawHardwareData, bool> _needHighSpeedCondition;
    private readonly TimeSpan _adjustmentInterval;

    public string Name => "AdaptiveRate";
    public int SampleRate => _currentSampleRate;

    /// <summary>
    /// 创建自适应采集策略
    /// </summary>
    /// <param name="minSampleRate">最小采样率</param>
    /// <param name="maxSampleRate">最大采样率</param>
    /// <param name="needHighSpeedCondition">需要高速采集的条件判断</param>
    /// <param name="adjustmentInterval">调整间隔</param>
    public AdaptiveRateStrategy(
        int minSampleRate,
        int maxSampleRate,
        Func<RawHardwareData, bool> needHighSpeedCondition,
        TimeSpan? adjustmentInterval = null)
    {
        if (minSampleRate <= 0)
            throw new ArgumentException("Min sample rate must be positive", nameof(minSampleRate));
        if (maxSampleRate <= minSampleRate)
            throw new ArgumentException("Max sample rate must be greater than min", nameof(maxSampleRate));

        _minSampleRate = minSampleRate;
        _maxSampleRate = maxSampleRate;
        _currentSampleRate = minSampleRate;
        _needHighSpeedCondition = needHighSpeedCondition ?? throw new ArgumentNullException(nameof(needHighSpeedCondition));
        _adjustmentInterval = adjustmentInterval ?? TimeSpan.FromSeconds(1);
    }

    public IObservable<RawHardwareData> CreateAcquisitionStream(
        IHardwareAdapter adapter,
        IScheduler scheduler,
        CancellationToken cancellationToken)
    {
        return Observable.Create<RawHardwareData>(observer =>
        {
            var disposables = new CompositeDisposable();
            var intervalDisposable = new SerialDisposable();
            disposables.Add(intervalDisposable);

            // 创建初始采集流
            var updateStream = () =>
            {
                var interval = TimeSpan.FromTicks(Stopwatch.Frequency / _currentSampleRate);

                var stream = Observable
                    .Generate(
                        0L,
                        _ => !cancellationToken.IsCancellationRequested,
                        tick => tick + 1,
                        _ =>
                        {
                            var data = adapter.ReadData();

                            // 每隔一段时间检查是否需要调整采样率
                            if (_ % (int)(_adjustmentInterval.TotalSeconds * _currentSampleRate) == 0)
                            {
                                AdjustSampleRate(data);
                            }

                            return data;
                        },
                        _ => interval)
                    .ObserveOn(scheduler)
                    .Subscribe(observer);

                intervalDisposable.Disposable = stream;
            };

            // 启动采集
            updateStream();

            return disposables;
        });
    }

    private void AdjustSampleRate(RawHardwareData data)
    {
        var needHighSpeed = _needHighSpeedCondition(data);

        if (needHighSpeed && _currentSampleRate < _maxSampleRate)
        {
            // 提高采样率
            _currentSampleRate = Math.Min(_currentSampleRate * 2, _maxSampleRate);
#if DEBUG
            Debug.WriteLine($"自适应策略: 提高采样率到 {_currentSampleRate} Hz");
#endif
        }
        else if (!needHighSpeed && _currentSampleRate > _minSampleRate)
        {
            // 降低采样率
            _currentSampleRate = Math.Max(_currentSampleRate / 2, _minSampleRate);
#if DEBUG
            Debug.WriteLine($"自适应策略: 降低采样率到 {_currentSampleRate} Hz");
#endif
        }
    }
}
