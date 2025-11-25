using MCCS.Collecter.DllNative;
using MCCS.Collecter.DllNative.Models;
using MCCS.Collecter.HardwareDevices;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MCCS.Collecter.ControllerManagers.Entities
{
    public sealed class BwControllerHardwareDevice : ControllerHardwareDeviceBase, IController
    {
        private readonly IDisposable _acquisitionSubscription;
        private readonly EventLoopScheduler _highPriorityScheduler; 
        private readonly HardwareDeviceConfiguration _hardwareDeviceConfiguration;
        private nint _singleBuffer = nint.Zero;
        private static readonly int _structSize = Marshal.SizeOf(typeof(TNet_ADHInfo));
        private ValveStatusEnum _valveStatus;

        public BwControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _hardwareDeviceConfiguration = configuration; 
            _highPriorityScheduler = CreateHighPriorityScheduler();
            _acquisitionSubscription = CreateAcquisitionLoop();
            _statusSubscription = Observable.Interval(TimeSpan.FromSeconds(configuration.StatusInterval))
                .Subscribe(onNext: c =>
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
        /// <summary>
        /// 因为现在批量采集就放在了Controller中;
        /// </summary>
        /// <returns></returns>
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
                        var errorReading = new DataPoint<List<TNet_ADHInfo>>
                        {
                            Value = [],
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
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
        private DataPoint<List<TNet_ADHInfo>> AcquireReading()
        {
            uint count = 0;
            if (POPNetCtrl.NetCtrl01_GetAD_HDataCount(_deviceHandle, ref count) != AddressContanst.OP_SUCCESSFUL || count == 0)
                return CreateBadDataPoint();
            if (_singleBuffer == nint.Zero)
                _singleBuffer = BufferPool.Rent();
            var results = new List<TNet_ADHInfo>((int)count);
            for (uint i = 0; i < count; i++)
            {
                if (POPNetCtrl.NetCtrl01_GetAD_HInfo(_deviceHandle, _singleBuffer, (uint)_structSize) != AddressContanst.OP_SUCCESSFUL)
                    return CreateBadDataPoint();
                var tempValue = Marshal.PtrToStructure<TNet_ADHInfo>(_singleBuffer);
                results.Add(tempValue);
            }

            return new DataPoint<List<TNet_ADHInfo>>
            {
                DeviceId = DeviceId,
                Value = results,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DataQuality = DataQuality.Good
            };
        }

        private static DataPoint<List<TNet_ADHInfo>> CreateBadDataPoint() => new()
        {
            Value = [],
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            DataQuality = DataQuality.Bad
        };

        #endregion

        public bool ConnectToHardware()
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

        public bool DisconnectFromHardware()
        {
            if (_deviceHandle == nint.Zero) return false;
            // 软件退出（关闭阀台，DA=0）
            POPNetCtrl.NetCtrl01_Soft_Ext(_deviceHandle);
            var result = POPNetCtrl.NetCtrl01_DisConnectToDev(_deviceHandle);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
#if DEBUG
                Debug.WriteLine("✓ 设备断开成功");
#endif
                _deviceHandle = nint.Zero;
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

        public bool OperationTest(uint isStart)
        {
            if (Status != HardwareConnectionStatus.Connected) return false;
            var success = POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, (byte)isStart);
            return success == AddressContanst.OP_SUCCESSFUL;
        }

        public ValveStatusEnum GetValveStatus()
        { 
            return _valveStatus;
        } 

        public int SetValveStatus(bool isOpen)
        {
            if (Status != HardwareConnectionStatus.Connected) return AddressContanst.DEVICE_NOT_CONNECTED;
            var v = isOpen ? 1u : 0u;
            var result = POPNetCtrl.NetCtrl01_Set_StationCtrl(_deviceHandle, v, 0);
            if (result == AddressContanst.OP_SUCCESSFUL) _valveStatus = isOpen ? ValveStatusEnum.Opened : ValveStatusEnum.Closed;
            return result;
        } 

        public bool SetControlState(SystemControlState controlMode)
        {
            if (Status != HardwareConnectionStatus.Connected) return false;
            var result = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)controlMode);
            return result == AddressContanst.OP_SUCCESSFUL;
        }

        public int SetStaticControlMode(StaticControlParams param)
        {   
            return POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, (uint)param.StaticLoadControl, param.Speed, param.TargetValue);
        }

        public int SetValleyPeakFilterNum(int freq)
        {  
            return POPNetCtrl.NetCtrl01_bWriteAddr(_deviceHandle, AddressContanst.Addr_ValleyPeak_FilterNum, (byte)freq);
        }

        public int SetDynamicControlMode(DynamicControlParams param)
        { 
            return POPNetCtrl.NetCtrl01_Osci_SetWaveInfo(_hardwareDeviceConfiguration.DeviceAddressId, param.MeanValue,
                param.Amplitude,
                param.Frequency,
                (byte)param.WaveType,
                (byte)param.ControlMode,
                param.CompensateAmplitude,
                param.CompensationPhase,
                param.CycleCount,
                (int)param.CtrlOpt);
        }

        public int SetDynamicStopControl(int tmpActMode, int tmpHaltState)
        {
            return POPNetCtrl.NetCtrl01_Osci_SetHaltState(_hardwareDeviceConfiguration.DeviceAddressId, (byte)tmpActMode, (byte)tmpHaltState);
        }

        public int SetSignalTare(int controlType)
        {
            if (ControlState != SystemControlState.Static) return 10;
            var staticState = GetStaticLoadControl();
            if (staticState is StaticLoadControlEnum.CTRLMODE_LoadS or StaticLoadControlEnum.CTRLMODE_LoadSVNP
                or StaticLoadControlEnum.CTRLMODE_TRACES) return 20;
            var res = POPNetCtrl.NetCtrl01_Set_offSet(_deviceHandle, (byte)controlType);
            return res;
        }

        public StaticLoadControlEnum GetStaticLoadControl()
        {
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_S_CtrlMode); 
            // TODO: 需要做成异步操作
            Thread.Sleep(200); 
            byte result = 0;
            POPNetCtrl.NetCtrl01_bReadAddrVal(_deviceHandle, AddressContanst.Addr_S_CtrlMode, ref result);
            return (StaticLoadControlEnum)result;
        }

        public void CleanupResources()
        {
            if (_singleBuffer != nint.Zero)
            {
                BufferPool.Return(_singleBuffer);
                _singleBuffer = nint.Zero;
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
