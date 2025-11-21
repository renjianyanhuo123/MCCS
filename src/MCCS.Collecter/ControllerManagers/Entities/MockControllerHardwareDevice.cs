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

        private readonly object _lock = new();

        private const float ForceMin = -10000.0f;
        private const float ForceMax = 10000.0f;

        private const float PositionMin = -1000.0f;
        private const float PositionMax = 1000.0f;
        private int _valleyPeakFilterNum = 0;
        private CancellationTokenSource? _forceStaticCTS;
        private CancellationTokenSource? _positionStaticCTS;
        private CancellationTokenSource? _dynamicCTS;

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

        private void GenerateWaveform(int changeType, double amplitude, double frequency, int waveType, uint totalCount, CancellationToken token)
        {
            double time = 0; // 时间变量（秒）

            while (time <= totalCount && !token.IsCancellationRequested)
            {
                // 根据波形类型计算值
                var value = waveType switch
                {
                    0 => // 正弦波
                        amplitude * Math.Sin(2 * Math.PI * frequency * time),
                    1 => // 三角波
                        CalculateTriangleWave(amplitude, frequency, time),
                    2 => // 方波
                        CalculateSquareWave(amplitude, frequency, time),
                    _ => 0
                };
                // 更新外部变量 位移 
                if (changeType == 0)
                {
                    lock (_lock)
                    {
                        _position = (float)value;
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        _force = (float)value;
                    }
                }
                var temp = 1 / frequency * 1000;
                // 等待500ms
                Thread.Sleep(500);

                // 时间递增
                time += temp;
            }
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
            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (speed < 0 && Math.Abs(_force - target) < Math.Abs(speed)
                        || speed > 0 && Math.Abs(_force - target) < speed)
                    {
                        lock (_lock)
                        {
                            _force = target;
                        }
                        break;
                    }
                    Thread.Sleep(1000);
                    lock (_lock)
                    {
                        _force += speed;
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
            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (speed < 0 && Math.Abs(_position - target) < Math.Abs(speed)
                        || speed > 0 && Math.Abs(_position - target) < speed)
                    {
                        lock (_lock)
                        {
                            _position = target;
                        }
                        break;
                    }
                    Thread.Sleep(1000);
                    lock (_lock)
                    {
                        _position += speed;
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
                    // Debug.WriteLine($"生成的模拟数据:{JsonConvert.SerializeObject(temp)}");
#endif
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataPoint<List<TNet_ADHInfo>> MockAcquireReading()
        {
            // 模拟数据采集
            var rand = new Random();
            var res = new List<TNet_ADHInfo>();
            var mockValue = new TNet_ADHInfo()
            {
                Net_AD_N =
                {
                    [0] = (float)AddNormalNoise(_force),
                    [1] = (float)AddNormalNoise(_force),
                    [2] = (float)(rand.NextDouble() * 100),
                    [3] = (float)(rand.NextDouble() * 100),
                    [4] = (float)(rand.NextDouble() * 100),
                    [5] = (float)(rand.NextDouble() * 100)
                },
                Net_AD_S =
                {
                    [0] = (float)AddNormalNoise(_position),
                    [1] = (float)AddNormalNoise(_position)
                }
            };
            res.Add(mockValue);
            return new DataPoint<List<TNet_ADHInfo>>
            {
                DeviceId = DeviceId,
                Value = res,
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
                    PositionChangeToTarget(speed, speed > 0 ? PositionMax : PositionMin);
                    ForceChangeToTarget(speed, controlParams.TargetValue);
                    break;
                case StaticLoadControlEnum.CTRLMODE_LoadNVSP:
                    speed = controlParams.TargetValue > _position ? speed : -speed;
                    PositionChangeToTarget(speed, controlParams.TargetValue);
                    ForceChangeToTarget(speed, speed > 0 ? ForceMax : ForceMin);
                    break;
                case StaticLoadControlEnum.CTRLMODE_OPEN:
                    speed = speed switch
                    {
                        < -1000.0f => -1000.0f,
                        > 1000.0f => 1000.0f,
                        _ => speed
                    };
                    PositionChangeToTarget(speed, speed > 0 ? PositionMax : PositionMin);
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
            }

            _dynamicCTS = new CancellationTokenSource();
            var token = _dynamicCTS.Token;
            Task.Run(() =>
            {
                GenerateWaveform(param.ControlMode, param.Amplitude, param.Frequency, param.WaveType, (uint)param.CycleCount, token);
            }, token);
            return AddressContanst.OP_SUCCESSFUL;
        }

        public int SetDynamicStopControl(int tmpActMode, int tmpHaltState)
        {
            _dynamicCTS?.Cancel();
            _dynamicCTS?.Dispose();
            _dynamicCTS = null;
            return AddressContanst.OP_SUCCESSFUL;
        }

        private void MockStaticControlStop()
        {
            // 默认位移保持
            _positionStaticCTS?.Cancel();
            _positionStaticCTS?.Dispose();
            _forceStaticCTS?.Cancel();
            _forceStaticCTS?.Dispose();
            _positionStaticCTS = null;
            _forceStaticCTS = null;
        } 

        public override void Dispose()
        {
            base.Dispose();
            _acquisitionSubscription.Dispose();
            _dataSubject?.OnCompleted();
            _dataSubject?.Dispose();
        }
    }
}
