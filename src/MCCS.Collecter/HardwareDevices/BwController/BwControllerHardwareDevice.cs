using MCCS.Collecter.DllNative;
using MCCS.Collecter.DllNative.Models;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.HardwareDevices.BwController
{
    public sealed class BwControllerHardwareDevice : ControllerHardwareDeviceBase
    { 
        private readonly IDisposable _acquisitionSubscription;
        private readonly EventLoopScheduler _highPriorityScheduler;
        private readonly int _sampleRate;
        private readonly HardwareDeviceConfiguration _hardwareDeviceConfiguration;
        private IntPtr _singleBuffer = IntPtr.Zero;
        private static readonly int _structSize = Marshal.SizeOf(typeof(TNet_ADHInfo)); 

        public BwControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _hardwareDeviceConfiguration = configuration; 
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

        #region Control Method 
        public override bool OperationTest(uint isStart)
        {
            if (Status != HardwareConnectionStatus.Connected) return false; 
            var success = POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, (byte)isStart);
            return success == AddressContanst.OP_SUCCESSFUL;
        }
        
        public override bool OperationValveState(bool isOpen)
        {
            if (Status != HardwareConnectionStatus.Connected) return false;
            var v = isOpen ? 1u : 0u;
            var result = POPNetCtrl.NetCtrl01_Set_StationCtrl(_deviceHandle, v, 0);
            return result == AddressContanst.OP_SUCCESSFUL;
        }
         
        public override bool OperationControlMode(SystemControlState controlState)
        {
            if (Status != HardwareConnectionStatus.Connected) return false;
            var result = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)controlState);
            return result == AddressContanst.OP_SUCCESSFUL;
        } 

        public override bool ManualControl(float outValue)
        {
            if (Status != HardwareConnectionStatus.Connected) return false;
            if (ControlState != SystemControlState.Static)
            {
                var setCtrlstateResult = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)SystemControlState.Static);
                if(setCtrlstateResult != AddressContanst.OP_SUCCESSFUL) return false;
                ControlState = SystemControlState.Static;
            }
            var setCtrlModeResult = POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, (uint)StaticLoadControlEnum.CTRLMODE_LoadS, outValue, 0);
            return setCtrlModeResult == AddressContanst.OP_SUCCESSFUL; 
        }
         
        public override bool StaticControl(StaticControlParams controlParams)
        {
            if (Status != HardwareConnectionStatus.Connected) return false;
            if (ControlState != SystemControlState.Static)
            {
                var setCtrlstateResult = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)SystemControlState.Static);
                if (setCtrlstateResult != AddressContanst.OP_SUCCESSFUL) return false;
                ControlState = SystemControlState.Static;
            }
            var result = POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, (uint)controlParams.StaticLoadControl, controlParams.Speed, controlParams.TargetValue);
            return result == AddressContanst.OP_SUCCESSFUL;
        }
        
        public override bool DynamicControl(DynamicControlParams controlParams)
        {
            if (Status != HardwareConnectionStatus.Connected) return false;
            if (ControlState != SystemControlState.Dynamic)
            {
                var setCtrlstateResult = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)SystemControlState.Dynamic);
                if (setCtrlstateResult != AddressContanst.OP_SUCCESSFUL) return false;
                ControlState = SystemControlState.Dynamic;
            }
            var result = POPNetCtrl.NetCtrl01_Osci_SetWaveInfo((int)DeviceId, 
                controlParams.MeanValue, 
                controlParams.Amplitude, 
                controlParams.Frequency, 
                (byte)controlParams.WaveType, 
                (byte)controlParams.ControlMode, 
                controlParams.CompensateAmplitude,
                controlParams.CompensationPhase,
                controlParams.CycleCount,
                controlParams.IsAdjustedMedian ? 0: 1 
                );
            return result == AddressContanst.OP_SUCCESSFUL;
        }

        #endregion

        #region Private Method
        private EventLoopScheduler CreateHighPriorityScheduler()
        {
            return new EventLoopScheduler(ts => new Thread(ts)
            {
                Name = $"Controller_{_hardwareDeviceConfiguration.DeviceId}",
                IsBackground = true,
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
                    _ =>
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
