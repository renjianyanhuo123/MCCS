using MCCS.Collecter.DllNative;
using MCCS.Collecter.DllNative.Models;
using MCCS.Collecter.HardwareDevices;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace MCCS.Collecter.ControllerManagers.Entities
{
    public sealed class MockControllerHardwareDevice : ControllerHardwareDeviceBase, IController
    {
        private readonly IDisposable _acquisitionSubscription; 
        private static readonly Random _rand = new();

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

        private StaticLoadControlEnum _staticControl = StaticLoadControlEnum.CTRLMODE_LoadS;
        /// <summary>
        /// 阀门状态
        /// </summary>
        private ValveStatusEnum _valveStatus;
        public MockControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        { 
            _acquisitionSubscription = CreateAcquisitionLoop();
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
                    bool reach;
                    lock (_lock)
                    {
                        reach = speed < 0 && Math.Abs(_force - target) < Math.Abs(speed * 10)
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

                    bool reach;
                    lock (_lock)
                    {
                        reach = speed < 0 && Math.Abs(_position - target) < Math.Abs(speed * 10)
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

        private IDisposable CreateAcquisitionLoop()
        {
            var t = 1.0 / _sampleRate * 1.0;
            return Observable.Interval(TimeSpan.FromSeconds(t))
                .Where(_ => _isRunning && Status == HardwareConnectionStatus.Connected)
                .Subscribe(_ =>
                {
                    _dataSubject.OnNext(MockAcquireReading());
#if DEBUG
                    // Debug.WriteLine($"运行位移:{_position}mm");
#endif
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataPoint<List<TNet_ADHInfo>> MockAcquireReading()
        {
            // 模拟数据采集
            var rand = new Random();
            var res = new List<TNet_ADHInfo>();
            // 根据当前时间模拟 100ms 更新一次数据的话，那么每次相当于要模拟出来100条数据
            // 根据速度计算   
            var mockValue = new TNet_ADHInfo()
            {
                Net_AD_N =
                {
                    [0] = (float)AddNormalNoise(_force, 0.001),
                    [1] = (float)AddNormalNoise(_force, 0.001),
                    [2] = (float)(rand.NextDouble() * 100),
                    [3] = (float)(rand.NextDouble() * 100),
                    [4] = (float)(rand.NextDouble() * 100),
                    [5] = (float)(rand.NextDouble() * 100)
                },
                Net_AD_S =
                {
                    [0] = (float)AddNormalNoise(_position, 0.001),
                    [1] = (float)AddNormalNoise(_position, 0.001)
                }
            };
            res.Add(mockValue);
            return new DataPoint<List<TNet_ADHInfo>>
            {
                DeviceId = DeviceId,
                Value = res,
                Unit = "",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DataQuality = DataQuality.Good
            };
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
            base.Dispose();
            MockStaticControlStop();
            SetDynamicStopControl(0,0);
            _acquisitionSubscription.Dispose();
            _dataSubject?.OnCompleted();
            _dataSubject?.Dispose();
        }
    }
}
