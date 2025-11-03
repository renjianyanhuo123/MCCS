using MCCS.Collecter.DllNative.Models;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.HardwareDevices.BwController
{
    public sealed class MockControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        private readonly IDisposable _acquisitionSubscription; 
        private readonly int _sampleRate;
        private static readonly Random _rand = new();

        // 目前一个控制器就控制一个作动器
        private float _force = 10.0f;
        private float _position = 50.0f;

        private readonly object _lock = new();

        private const float ForceMin = -10000.0f;
        private const float ForceMax = 10000.0f;

        private const float PositionMin = -1000.0f;
        private const float PositionMax = 1000.0f;

        public MockControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        { 
            _sampleRate = configuration.Signals.Max(s => s.SampleRate); 
            _acquisitionSubscription = CreateAcquisitionLoop(); 
        } 

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

        private void GenerateWaveform(int changeType, double amplitude, double frequency, int waveType, uint totalCount = 0)
        {
            double time = 0; // 时间变量（秒）

            while (time <= totalCount)
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
            var phase = (time % period) / period; // 归一化到 [0, 1)

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
            var phase = (time % period) / period; // 归一化到 [0, 1)

            // 前半个周期为正幅值，后半个周期为负幅值
            return phase < 0.5 ? amplitude : -amplitude;
        }

        public override bool ConnectToHardware()
        {
            Status = HardwareConnectionStatus.Connected;
            return true;
        }

        public override bool DisconnectFromHardware()
        {
            Status = HardwareConnectionStatus.Disconnected;
            return true;
        }

        public override bool OperationTest(uint isStart)
        {
            return true;
        }

        public override bool OperationValveState(bool isOpen)
        {
            return true;
        }

        public override bool OperationControlMode(SystemControlState controlState)
        {
            ControlState = controlState;
            return true;
        }

        public override DeviceCommandContext ManualControl(long deviceId, float outValue)
        {
            // 创建或获取设备上下文
            var context = _deviceContexts.GetOrAdd(deviceId, new DeviceCommandContext
            {
                DeviceId = deviceId,
                IsValid = false
            });
            if (Status != HardwareConnectionStatus.Connected) return context;
            ControlState = SystemControlState.Static;
            var speed = outValue / 60.0f;
            speed = speed switch
            {
                < -1000.0f => -1000.0f,
                > 1000.0f => 1000.0f,
                _ => speed
            };
            PositionChangeToTarget(speed, speed > 0 ? PositionMax : PositionMin);
            return context;
        }

        public override DeviceCommandContext StaticControl(StaticControlParams controlParams)
        {
            var context = _deviceContexts.GetOrAdd(controlParams.DeviceId, new DeviceCommandContext
            {
                DeviceId = controlParams.DeviceId,
                IsValid = false
            });
            if (Status != HardwareConnectionStatus.Connected) return context;
            ControlState = SystemControlState.Static;
            var speed = controlParams.Speed / 60.0f;
            switch (controlParams.StaticLoadControl)
            {
                case StaticLoadControlEnum.CTRLMODE_LoadN:
                    ForceChangeToTarget(speed, controlParams.TargetValue);
                    break;
                case StaticLoadControlEnum.CTRLMODE_LoadS:
                    PositionChangeToTarget(speed, controlParams.TargetValue);
                    break;
                case StaticLoadControlEnum.CTRLMODE_LoadSVNP:
                    PositionChangeToTarget(speed - 1, speed - 1 > 0 ? PositionMax : PositionMin);
                    ForceChangeToTarget(speed, controlParams.TargetValue);
                    break;
                case StaticLoadControlEnum.CTRLMODE_LoadNVSP:
                    PositionChangeToTarget(speed, controlParams.TargetValue);
                    ForceChangeToTarget(speed - 1, speed - 1 > 0 ? ForceMax : ForceMin);
                    break;
                default:
                    break;
            } 
            return context;
        }

        public override DeviceCommandContext DynamicControl( DynamicControlParams controlParams)
        {
            var context = _deviceContexts.GetOrAdd(controlParams.DeviceId, new DeviceCommandContext
            {
                DeviceId = controlParams.DeviceId,
                IsValid = false
            });
            if (Status != HardwareConnectionStatus.Connected) return context;
            ControlState = SystemControlState.Dynamic;
            Task.Run(() =>
            {
                GenerateWaveform(controlParams.ControlMode, controlParams.Amplitude, controlParams.Frequency, controlParams.WaveType, (uint)controlParams.CycleCount);
            });
            return context;
        }

        private void ForceChangeToTarget(float speed, float target)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if ((speed < 0 && Math.Abs(_force - target) < Math.Abs(speed)) 
                        || (speed > 0 && Math.Abs(_force - target) < speed))
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
            });
        }

        private void PositionChangeToTarget(float speed, float target)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if ((speed < 0 && Math.Abs(_position - target) < Math.Abs(speed))
                        || (speed > 0 && Math.Abs(_position - target) < speed))
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
            });
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
        private DataPoint MockAcquireReading()
        {
            // 模拟数据采集
            var rand = new Random();
            var res = new List<BatchCollectItemModel>();
            var mockValue = new TNet_ADHInfo
            {
                Net_AD_N =
                {
                    [0] = (float)(AddNormalNoise(_force)),
                    [1] = (float)(AddNormalNoise(_force)),
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
            res.Add(StructDataToCollectModel(mockValue));
            return new DataPoint
            {
                DeviceId = DeviceId,
                Value = res,
                Timestamp = Stopwatch.GetTimestamp(),
                DataQuality = DataQuality.Good
            };
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
