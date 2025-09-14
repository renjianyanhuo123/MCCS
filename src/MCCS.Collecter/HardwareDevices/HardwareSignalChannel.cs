using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace MCCS.Collecter.HardwareDevices
{
    public class HardwareSignalChannel : IDisposable
    {
        private readonly HardwareSignalConfiguration _signalConfig;

        // 使用ReplaySubject支持背压和缓冲
        private readonly ReplaySubject<DataPoint> _dataSubject; 
        private readonly IDisposable _acquisitionSubscription;
        private readonly EventLoopScheduler _highPriorityScheduler;
        private volatile bool _isRunning;

        public string SignalId => _signalConfig.SignalId;
        public HardwareSignalConfiguration SignalConfig => _signalConfig;

        /// <summary>
        /// 该信号的独立数据流 - 无全局汇总开销
        /// </summary>
        public IObservable<DataPoint> DataStream => _dataSubject.AsObservable();

        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            _signalConfig = signalConfig; 
            _highPriorityScheduler = CreateHighPriorityScheduler(); 
            // 创建数据采集订阅
            _acquisitionSubscription = CreateAcquisitionLoop();
            // 使用ReplaySubject以支持晚加入的订阅者获取最新数据
            _dataSubject = new ReplaySubject<DataPoint>(signalConfig.BufferSize);
        }
        private EventLoopScheduler CreateHighPriorityScheduler()
        {
            return new EventLoopScheduler(ts => new Thread(ts)
            {
                Name = $"DAQ_Channel_{_signalConfig.SignalId}",
                IsBackground = false,
                Priority = ThreadPriority.Highest
            });
        }
        private IDisposable CreateAcquisitionLoop()
        {
            // 创建精确定时的数据采集循环
            return Observable
                .Generate(
                    0L, // 初始状态
                    _ => _isRunning, // 继续条件
                    tick => tick + 1, // 状态更新
                    tick => AcquireReading(), // 结果选择器
                    tick => CalculateNextInterval()) // 时间选择器
                .ObserveOn(_highPriorityScheduler)
                .Subscribe(
                    reading => _dataSubject.OnNext(reading),
                    error =>
                    { 
                        // 发送错误数据点而不是停止流
                        var errorReading = new DataPoint
                        {
                            SignalId = _signalConfig.SignalId, 
                            Value = double.NaN,
                            Timestamp = Stopwatch.GetTimestamp(), 
                            DataQuality = DataQuality.Bad
                        };
                        _dataSubject.OnNext(errorReading);
                    });
        }
        private TimeSpan CalculateNextInterval()
        {
            // 精确计算下次采样间隔
            return TimeSpan.FromTicks(Stopwatch.Frequency / _signalConfig.SampleRate);
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DataPoint AcquireReading()
        {
            try
            {
                // 模拟高速硬件读取
                var rawValue = SimulateHardwareRead();
                var timestamp = Stopwatch.GetTimestamp();
                var quality = ValidateReading(rawValue) ? DataQuality.Good : DataQuality.Uncertain;

                return new DataPoint
                {
                    Value = rawValue,
                    Timestamp = timestamp,
                    DataQuality = quality
                };
            }
            catch
            {
                return new DataPoint
                {
                    Value = 0,
                    Timestamp = Stopwatch.GetTimestamp(),
                    DataQuality = DataQuality.Bad
                };
            }
        }
        private static bool ValidateReading(double value)
        {
            return !double.IsNaN(value) && value >= _signalConfig.MinValue && value <= _signalConfig.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double SimulateHardwareRead()
        {
            // 模拟实际硬件读取逻辑
            return Math.Sin(DateTime.Now.Ticks * 0.00001) * (_signalConfig.MaxValue - _signalConfig.MinValue) / 2
                   + (_signalConfig.MaxValue + _signalConfig.MinValue) / 2;
        }

        public void Start()
        {
            _isRunning = true;
        } 
         
        public void Stop()
        {
            _isRunning = false;
        }

        public void Dispose()
        {
            Stop();
            _acquisitionSubscription?.Dispose();
            _dataSubject?.OnCompleted();
            _dataSubject?.Dispose();
            _highPriorityScheduler?.Dispose();
        }
    }
}
