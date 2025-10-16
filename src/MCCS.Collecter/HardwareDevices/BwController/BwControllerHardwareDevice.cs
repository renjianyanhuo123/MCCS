using MCCS.Collecter.DllNative;
using MCCS.Collecter.DllNative.Models;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MCCS.Collecter.HardwareDevices.BwController
{
    public sealed class BwControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        private readonly ReplaySubject<DataPoint> _dataSubject;
        private readonly IDisposable _acquisitionSubscription;
        private readonly EventLoopScheduler _highPriorityScheduler;
        private readonly int _sampleRate;
        private readonly HardwareDeviceConfiguration _hardwareDeviceConfiguration;

        public BwControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _hardwareDeviceConfiguration = configuration;
            _dataSubject = new ReplaySubject<DataPoint>(bufferSize: 1000);
            _sampleRate = configuration.Signals.Max(s => s.SampleRate);
            _highPriorityScheduler = CreateHighPriorityScheduler();
            _acquisitionSubscription = CreateAcquisitionLoop();
            _statusSubscription = Observable.Interval(TimeSpan.FromSeconds(configuration.StatusInterval))
                .Subscribe(onNext:c =>
                {
                    uint t = 0;
                    POPNetCtrl.NetCtrl01_ReadConectState(_deviceHandle, ref t);
                    var res = t switch
                    {
                        0 => HardwareConnectionStatus.Connected,
                        1 => HardwareConnectionStatus.Disconnected,
                        2 => HardwareConnectionStatus.Error,
                        _ => HardwareConnectionStatus.Disconnected
                    };
                    _statusSubject.OnNext(res);
                }, onError: exception =>
                {
                    _statusSubject.OnNext(HardwareConnectionStatus.Disconnected);
                });
        }

        public override bool ConnectToHardware()
        {
            var result = POPNetCtrl.NetCtrl01_ConnectToDev(DeviceId, ref _deviceHandle);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
#if DEBUG
                Debug.WriteLine($"✓ 设备连接成功，句柄: 0x{DeviceId:X}");
#endif 
                return true;
            }
            else
            {
#if DEBUG
                Debug.WriteLine($"✗ 设备连接失败，错误码: {result}");
#endif
                return false;
            }
        }

        public override bool DisconnectFromHardware()
        {
            if (_deviceHandle == IntPtr.Zero) return false;
            // 软件退出（关闭阀台，DA=0）
            POPNetCtrl.NetCtrl01_Soft_Ext(_deviceHandle); 
            var result = POPNetCtrl.NetCtrl01_DisConnectToDev(_deviceHandle);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
#if DEBUG
                Debug.WriteLine("✓ 设备断开成功");
#endif
                _deviceHandle = IntPtr.Zero;
                return true;
            }
            else
            {
#if DEBUG
                Debug.WriteLine($"✗ 设备断开失败，错误码: {result}");
#endif
                return false;
            }
        }

        public IObservable<DataPoint> DataStream => _dataSubject.AsObservable(); 
        #region Private Method
        private EventLoopScheduler CreateHighPriorityScheduler()
        {
            return new EventLoopScheduler(ts => new Thread(ts)
            {
                Name = $"Controller_{_hardwareDeviceConfiguration.DeviceId}",
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
                    _dataSubject.OnNext,
                    error =>
                    {
                        // 发送错误数据点而不是停止流
                        var errorReading = new DataPoint
                        { 
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
            return TimeSpan.FromTicks(Stopwatch.Frequency / _sampleRate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataPoint AcquireReading()
        {
            if (_hardwareDeviceConfiguration.IsSimulation) return MockAcquireReading();
            IntPtr buffer = IntPtr.Zero;
            try
            {
                uint dataCount = 0;
                int result = POPNetCtrl.NetCtrl01_GetAD_HDataCount(_deviceHandle, ref dataCount);
                if(result != AddressContanst.OP_SUCCESSFUL) throw new Exception("读取数据计数失败");
                if( dataCount <= 0) return new DataPoint
                {
                    Value = 0,
                    Timestamp = Stopwatch.GetTimestamp(),
                    DataQuality = DataQuality.Bad
                };
                // 计算单个结构体大小
                int structSize = Marshal.SizeOf(typeof(TNet_ADHInfo));
                uint totalSize = (uint)(structSize * dataCount); 
                // 分配非托管内存
                buffer = BufferPool.Rent();
                // 初始化内存
                for (int i = 0; i < dataCount; i++)
                {
                    var initData = new TNet_ADHInfo();
                    IntPtr structPtr = IntPtr.Add(buffer, i * structSize);
                    Marshal.StructureToPtr(initData, structPtr, false);
                }
                // 调用DLL函数
                int result1 = POPNetCtrl.NetCtrl01_GetAD_HInfo(
                    _deviceHandle,
                    buffer,
                    totalSize
                );
                if (result1 != AddressContanst.OP_SUCCESSFUL)
                    throw new Exception($"读取数据失败，错误码: {result1}");
                // 从内存中读取数据
                var resultArray = new List<TNet_ADHInfo>();
                for (int i = 0; i < dataCount; i++)
                {
                    IntPtr structPtr = IntPtr.Add(buffer, i * structSize);
                    resultArray.Add(Marshal.PtrToStructure<TNet_ADHInfo>(structPtr));
                }
                return new DataPoint
                {
                    DeviceId = DeviceId,
                    Value = resultArray,
                    Timestamp = Stopwatch.GetTimestamp(),
                    DataQuality = DataQuality.Good
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
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    BufferPool.Return(buffer);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataPoint MockAcquireReading() 
        {
            // 模拟数据采集
            var rand = new Random();
            var res = new List<TNet_ADHInfo>();
            var mockValue = new TNet_ADHInfo();
            mockValue.Net_AD_N[0] = (float)(rand.NextDouble() * 100);
            mockValue.Net_AD_N[1] = (float)(rand.NextDouble() * 100);
            mockValue.Net_AD_N[2] = (float)(rand.NextDouble() * 100);
            mockValue.Net_AD_N[3] = (float)(rand.NextDouble() * 100);
            mockValue.Net_AD_N[4] = (float)(rand.NextDouble() * 100);
            mockValue.Net_AD_N[5] = (float)(rand.NextDouble() * 100);
            mockValue.Net_AD_S[0] = (float)(rand.NextDouble() * 100);
            mockValue.Net_AD_S[1] = (float)(rand.NextDouble() * 100);
            res.Add(mockValue);
            return new DataPoint
            {
                DeviceId = DeviceId,
                Value = res,
                Timestamp = Stopwatch.GetTimestamp(),
                DataQuality = DataQuality.Good
            };
        }
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _highPriorityScheduler?.Dispose();
            _acquisitionSubscription?.Dispose();
            _dataSubject?.Dispose();
        }
    }
}
