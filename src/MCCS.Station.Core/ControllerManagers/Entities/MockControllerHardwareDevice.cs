using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using MCCS.Station.Core.DllNative;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.ControllerManagers.Entities
{
    public sealed class MockControllerHardwareDevice : ControllerHardwareDeviceBase, IController
    {
        private readonly IDisposable _acquisitionSubscription;
        private readonly EventLoopScheduler _mockScheduler;
        private static readonly Random _rand = new();
        private long _sampleSequence = 0;

        // 目前一个控制器就控制一个作动器
        private float _force = 10.0f;
        private float _position = 50.0f;

        private float _absoultForce = 0.0f;
        private float _absoultPosition = 0.0f;

        private readonly object _lock = new(); 

        private const float _forceMin = -10000.0f;
        private const float _forceMax = 10000.0f;

        private const float _positionMin = -1000.0f;
        private const float _positionMax = 1000.0f;

        private int _valleyPeakFilterNum = 0;
        private CancellationTokenSource? _forceStaticCTS;
        private CancellationTokenSource? _positionStaticCTS;
        private CancellationTokenSource? _dynamicCTS;

        private PeriodicTimer? _forceTimer;
        private PeriodicTimer? _positionTimer;
        private PeriodicTimer? _dynamicTimer;
        private const double _samplingInterval = 0.01;

        // 预分配的缓冲区，用于减少GC压力
        // ReSharper disable once InconsistentNaming
        private const int _bufferPoolSize = 16;
        private readonly TNet_ADHInfo[][] _valueBufferPool;
        private readonly SampleBatch<TNet_ADHInfo>[] _batchBufferPool;
        private int _bufferIndex;

        private StaticLoadControlEnum _staticControl = StaticLoadControlEnum.CTRLMODE_LoadS;
        /// <summary>
        /// 阀门状态
        /// </summary>
        private ValveStatusEnum _valveStatus;

        public MockControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _mockScheduler = new EventLoopScheduler(ts => new Thread(ts)
            {
                Name = $"MockController_{configuration.DeviceId}",
                IsBackground = true
            });

            // 预分配缓冲区池，避免高频采集时的GC压力
            _valueBufferPool = new TNet_ADHInfo[_bufferPoolSize][];
            _batchBufferPool = new SampleBatch<TNet_ADHInfo>[_bufferPoolSize];
            for (var i = 0; i < _bufferPoolSize; i++)
            {
                _valueBufferPool[i] = new TNet_ADHInfo[2];
                for (var j = 0; j < 2; j++)
                {
                    _valueBufferPool[i][j] = new TNet_ADHInfo
                    {
                        Net_AD_N = new float[6],
                        Net_AD_S = new float[2]
                    };
                }
                _batchBufferPool[i] = new SampleBatch<TNet_ADHInfo>
                {
                    DeviceId = DeviceId,
                    Values = _valueBufferPool[i]
                };
            }

            // 创建批量采集流（热Observable）
            var acquisitionStream = CreateAcquisitionObservable().Publish();
            DataStream = acquisitionStream;

            // 从批量流展开为单条数据流
            IndividualDataStream = DataStream
                .SelectMany(batch => batch.Values.Select((value, index) => new DataPoint<TNet_ADHInfo>
                {
                    DeviceId = batch.DeviceId,
                    Timestamp = batch.ArrivalTicks + index,
                    SequenceIndex = batch.SequenceStart + index,
                    Value = value,
                    Unit = string.Empty 
                }))
                .Publish()
                .RefCount();

            _acquisitionSubscription = acquisitionStream.Connect();
        }

        #region 私有方法
        /// <summary>
        /// 根据正态分布对输入值进行微扰。
        /// </summary>
        /// <param name="value">输入原始值</param>
        /// <param name="sigma">标准差（扰动强度，0.001~0.01推荐）</param>
        /// <returns>输出带正态分布微扰的值</returns>
        private static double AddNormalNoise(float value, double sigma = 0.005)
        {
            // Box–Muller 变换产生标准正态分布 N(0,1)
            var u1 = 1.0 - _rand.NextDouble(); // 避免0
            var u2 = 1.0 - _rand.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            // 缩放扰动
            var noise = randStdNormal * sigma;

            return value + noise;
        }
         

        /// <summary>
        /// 计算三角波的值
        /// </summary>
        private double CalculateTriangleWave(double amplitude, double frequency, double time)
        {
            var period = 1.0 / frequency;
            var phase = time % period / period; // 归一化到 [0, 1)

            if (phase < 0.25)
            {
                // 上升段 0 -> amplitude
                return amplitude * (4 * phase);
            }

            if (phase < 0.75)
            {
                // 下降段 amplitude -> -amplitude
                return amplitude * (2 - 4 * phase);
            }

            // 上升段 -amplitude -> 0
            return amplitude * (4 * phase - 4);
        }

        /// <summary>
        /// 计算方波的值
        /// </summary>
        private double CalculateSquareWave(double amplitude, double frequency, double time)
        {
            var period = 1.0 / frequency;
            var phase = time % period / period; // 归一化到 [0, 1)

            // 前半个周期为正幅值，后半个周期为负幅值
            return phase < 0.5 ? amplitude : -amplitude;
        }
        private void ForceChangeToTarget(float speed, float target)
        {
            if (_forceStaticCTS != null)
            {
                _forceStaticCTS.Cancel();
                _forceStaticCTS?.Dispose();
            }
            _forceStaticCTS = new CancellationTokenSource();
            var token = _forceStaticCTS.Token;
            _forceTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(10)); 
            _ = Task.Run(async () =>
            {
                while (await _forceTimer.WaitForNextTickAsync(token))
                {
                    if (token.IsCancellationRequested) break;
                    lock (_lock)
                    {
                        var reach = speed < 0 && Math.Abs(_force - target) < Math.Abs(speed * 10)
                                    || speed > 0 && Math.Abs(_force - target) < speed * 10; 
                        if (reach)
                        {
                            _force = target;
                            break;
                        } 
                        _force += speed * 10;
                    }
                }
            }, token); 
        }

        private void PositionChangeToTarget(float speed, float target)
        {
            if (_positionStaticCTS != null)
            {
                _positionStaticCTS.Cancel();
                _positionStaticCTS?.Dispose();
            }
            _positionStaticCTS = new CancellationTokenSource();
            var token = _positionStaticCTS.Token;
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10)); 
            _ = Task.Run(async () =>
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    if (token.IsCancellationRequested) break;

                    lock (_lock)
                    {
                        var reach = speed < 0 && Math.Abs(_position - target) < Math.Abs(speed * 10)
                                    || speed > 0 && Math.Abs(_position - target) < speed * 10;

                        if (reach)
                        {
                            _position = target;
                            break;
                        }

                        _position += speed * 10;
                    }
                }
            }, token);
        }

        /// <summary>
        /// 创建模拟数据采集流
        /// 模拟真实设备的采集行为：每2ms采集一批数据
        /// </summary>
        private IObservable<SampleBatch<TNet_ADHInfo>> CreateAcquisitionObservable() =>
            Observable.Create<SampleBatch<TNet_ADHInfo>>(observer =>
            {
                return _mockScheduler.SchedulePeriodic(TimeSpan.FromMilliseconds(2), () =>
                {
                    if (!_isRunning || Status != HardwareConnectionStatus.Connected)
                        return;

                    var batch = MockAcquireReading();
                    if (batch != null)
                        observer.OnNext(batch); 
                });
            });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SampleBatch<TNet_ADHInfo>? MockAcquireReading()
        {
            // 模拟数据采集 - 每次生成少量样本（模拟真实采集频率）
            const uint sampleCount = 2; // 每批次2个样本，与2ms间隔配合模拟1000Hz采样率
#if DEBUG
            Console.WriteLine($"执行采集: {DateTime.Now:HH:mm:ss.fff}");
#endif
            // 使用环形缓冲区复用预分配的对象，避免GC压力
            var currentIndex = Interlocked.Increment(ref _bufferIndex) % _bufferPoolSize;
            var values = _valueBufferPool[currentIndex];
            var batch = _batchBufferPool[currentIndex];

            // 更新预分配对象的数据（原地修改，不分配新内存）
            for (var i = 0; i < sampleCount; i++)
            {
                values[i].Net_AD_N[0] = (float)AddNormalNoise(_force, 0.001);
                values[i].Net_AD_N[1] = (float)AddNormalNoise(_force, 0.001);
                values[i].Net_AD_N[2] = (float)(_rand.NextDouble() * 100);
                values[i].Net_AD_N[3] = (float)(_rand.NextDouble() * 100);
                values[i].Net_AD_N[4] = (float)(_rand.NextDouble() * 100);
                values[i].Net_AD_N[5] = (float)(_rand.NextDouble() * 100);
                values[i].Net_AD_S[0] = (float)AddNormalNoise(_position, 0.001);
                values[i].Net_AD_S[1] = (float)AddNormalNoise(_position, 0.001);
            }

            // 直接修改预分配的batch对象（零分配）
            batch.SampleCount = sampleCount;
            batch.ArrivalTicks = Stopwatch.GetTimestamp();
            batch.SequenceStart = Interlocked.Add(ref _sampleSequence, sampleCount) - sampleCount;

            return batch;
        }
        #endregion

        public bool ConnectToHardware()
        {
            Status = HardwareConnectionStatus.Connected;
            return true;
        }

        public bool DisconnectFromHardware()
        {
            Status = HardwareConnectionStatus.Disconnected;
            return true;
        }

        public bool OperationTest(uint isStart)
        {
            return true;
        }

        public ValveStatusEnum GetValveStatus()
        {
            return _valveStatus;
        }

        public int SetValveStatus(bool isOpen)
        {
            _valveStatus = isOpen ? ValveStatusEnum.Opened : ValveStatusEnum.Closed;
            return AddressContanst.OP_SUCCESSFUL;
        }

        public bool SetControlState(SystemControlState controlMode)
        {
            ControlState = controlMode;
            return true;
        }

        public int SetStaticControlMode(StaticControlParams controlParams)
        {
            if (Status != HardwareConnectionStatus.Connected) return AddressContanst.DEVICE_NOT_CONNECTED;
            ControlState = SystemControlState.Static;
            var speed = controlParams.Speed / 60.0f;
            speed /= 1000.0f;
            _staticControl = controlParams.StaticLoadControl;
            switch (controlParams.StaticLoadControl)
            {
                case StaticLoadControlEnum.CTRLMODE_LoadN:
                    speed = controlParams.TargetValue > _force ? speed : -speed;
                    ForceChangeToTarget(speed, controlParams.TargetValue);
                    break;
                case StaticLoadControlEnum.CTRLMODE_LoadS:
                    speed = controlParams.TargetValue > _position ? speed : -speed;
                    PositionChangeToTarget(speed, controlParams.TargetValue);
                    break;
                case StaticLoadControlEnum.CTRLMODE_LoadSVNP:
                    speed = controlParams.TargetValue > _force ? speed : -speed;
                    PositionChangeToTarget(speed, speed > 0 ? _positionMax : _positionMin);
                    ForceChangeToTarget(speed, controlParams.TargetValue);
                    break;
                case StaticLoadControlEnum.CTRLMODE_LoadNVSP:
                    speed = controlParams.TargetValue > _position ? speed : -speed;
                    PositionChangeToTarget(speed, controlParams.TargetValue);
                    ForceChangeToTarget(speed, speed > 0 ? _forceMax : _forceMin);
                    break;
                case StaticLoadControlEnum.CTRLMODE_OPEN:
                    speed = speed switch
                    {
                        < -1000.0f => -1000.0f,
                        > 1000.0f => 1000.0f,
                        _ => speed
                    };
                    PositionChangeToTarget(speed, speed > 0 ? _positionMax : _positionMin);
                    break; 
                case StaticLoadControlEnum.CTRLMODE_HLoadN: 
                case StaticLoadControlEnum.CTRLMODE_HLoadS:
                    MockStaticControlStop();
                    break;
                case StaticLoadControlEnum.CTRLMODE_None:
                case StaticLoadControlEnum.CTRLMODE_NOCTRL:
                case StaticLoadControlEnum.CTRLMODE_TRACEN:
                case StaticLoadControlEnum.CTRLMODE_TRACES:
                case StaticLoadControlEnum.CTRLMODE_HALTS:
                default:
                    break;
            } 
            return AddressContanst.OP_SUCCESSFUL;
        }

        public int SetValleyPeakFilterNum(int freq)
        {  
            _valleyPeakFilterNum = freq;
            return AddressContanst.OP_SUCCESSFUL;
        }

        public int SetDynamicControlMode(DynamicControlParams param)
        {
            if (Status != HardwareConnectionStatus.Connected) return AddressContanst.DEVICE_NOT_CONNECTED;
            ControlState = SystemControlState.Dynamic;
            if (_dynamicCTS != null)
            {
                _dynamicCTS.Cancel();
                _dynamicCTS?.Dispose();
                _dynamicCTS = null;
            }

            if (_dynamicTimer != null)
            {
                _dynamicTimer.Dispose();
                _dynamicTimer = null;
            } 
            _dynamicCTS = new CancellationTokenSource();
            _dynamicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
            var token = _dynamicCTS.Token;
            double time = 0; // seconds 
            double totalDuration = param.CycleCount / param.Frequency; // 总时长(秒) = 周期数 / 频率
            
            Task.Run(async () =>
            {
                while (time <= totalDuration && _dynamicTimer != null && await _dynamicTimer.WaitForNextTickAsync(token))
                {
                    if (token.IsCancellationRequested) break;
                    // 计算波形值
                    var value = param.WaveType switch
                    {
                        0 => param.Amplitude * Math.Sin(2 * Math.PI * param.Frequency * time) + param.MeanValue,
                        1 => CalculateTriangleWave(param.Amplitude, param.Frequency, time),
                        2 => CalculateSquareWave(param.Amplitude, param.Frequency, time),
                        _ => 0
                    };
#if DEBUG
                    Debug.WriteLine($"原始位移:{value:F2}mm");
#endif
                    // 更新变量
                    lock (_lock)
                    {
                        if (param.ControlMode == 0)
                            _position = (float)value;
                        else
                            _force = (float)value;
                    }

                    // 每10ms增加一次时间
                    time += _samplingInterval;
                }
            }, token);
            return AddressContanst.OP_SUCCESSFUL;
        }

        public int SetDynamicStopControl(int tmpActMode, int tmpHaltState)
        {
            _dynamicCTS?.Cancel();
            _dynamicCTS?.Dispose();
            _dynamicTimer?.Dispose();
            _dynamicTimer = null;
            _dynamicCTS = null;
            return AddressContanst.OP_SUCCESSFUL;
        }

        public int SetSignalTare(int controlType)
        {
            if (ControlState != SystemControlState.Static) return 10; 
            if (_staticControl is StaticLoadControlEnum.CTRLMODE_LoadS or StaticLoadControlEnum.CTRLMODE_LoadSVNP
                or StaticLoadControlEnum.CTRLMODE_TRACES) return 20;
            _absoultForce = _force;
            _absoultPosition = _position;
            _force = 0.0f;
            _position = 0.0f;
            return AddressContanst.OP_SUCCESSFUL;
        }

        public StaticLoadControlEnum GetStaticLoadControl()
        {
            return _staticControl;
        }

        private void MockStaticControlStop()
        {
            // 默认位移保持
            _positionTimer?.Dispose();
            _positionStaticCTS?.Cancel();
            _positionStaticCTS?.Dispose();
            _forceTimer?.Dispose();
            _forceStaticCTS?.Cancel();
            _forceStaticCTS?.Dispose();
            _dynamicCTS?.Dispose();
            _dynamicCTS = null;
            _positionStaticCTS = null;
            _forceStaticCTS = null;
            _positionTimer = null;
            _forceTimer = null;
        } 

        public override void Dispose()
        {
            _acquisitionSubscription?.Dispose();
            _mockScheduler?.Dispose();
            MockStaticControlStop();
            SetDynamicStopControl(0, 0);
            base.Dispose();
        }
    }
}
