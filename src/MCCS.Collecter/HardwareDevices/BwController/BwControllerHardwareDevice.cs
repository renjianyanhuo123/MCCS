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
        private IntPtr _singleBuffer = IntPtr.Zero;
        private static readonly int _structSize = Marshal.SizeOf(typeof(TNet_ADHInfo)); 

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
                    Status = res;
                    _statusSubject.OnNext(res);
                }, onError: _ =>
                {
                    _statusSubject.OnNext(HardwareConnectionStatus.Disconnected);
                });
        }

        public override bool ConnectToHardware()
        {
            var result = POPNetCtrl.NetCtrl01_ConnectToDev(_hardwareDeviceConfiguration.DeviceAddressId, ref _deviceHandle);
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
                    _ => AcquireReading(), // 结果选择器
                    _ => CalculateNextInterval()) // 时间选择器
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
            uint count = 0;
            if (POPNetCtrl.NetCtrl01_GetAD_HDataCount(_deviceHandle, ref count) != AddressContanst.OP_SUCCESSFUL || count == 0)
                return CreateBadDataPoint(); 
            if(_singleBuffer != IntPtr.Zero)
                _singleBuffer = BufferPool.Rent();
            var results = new List<BatchCollectItemModel>((int)count);
            for (uint i = 0; i < count; i++)
            {
                if (POPNetCtrl.NetCtrl01_GetAD_HInfo(_deviceHandle, _singleBuffer, (uint)_structSize) != AddressContanst.OP_SUCCESSFUL)
                    return CreateBadDataPoint();
                var tempValue = Marshal.PtrToStructure<TNet_ADHInfo>(_singleBuffer); 
                results.Add(StructDataToCollectModel(tempValue));
            }

            return new DataPoint
            {
                DeviceId = DeviceId,
                Value = results,
                Timestamp = Stopwatch.GetTimestamp(),
                DataQuality = DataQuality.Good
            };
        }

        private DataPoint CreateBadDataPoint() => new()
        {
            Value = 0,
            Timestamp = Stopwatch.GetTimestamp(),
            DataQuality = DataQuality.Bad
        };
        #endregion
        public void CleanupResources()
        {
            if (_singleBuffer != IntPtr.Zero)
            {
                BufferPool.Return(_singleBuffer);
                _singleBuffer = IntPtr.Zero;
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            _highPriorityScheduler?.Dispose();
            _acquisitionSubscription?.Dispose();
            _dataSubject?.Dispose();
            CleanupResources();
        }
    }
}
