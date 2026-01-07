using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using MCCS.Infrastructure.Helper;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using MCCS.Station.Core.DllNative;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.ControllerManagers.Entities
{
    public sealed class BwControllerHardwareDevice : ControllerHardwareDeviceBase, IController
    {
        private readonly IDisposable _acquisitionSubscription;
        private readonly EventLoopScheduler _highPriorityScheduler; 
        private readonly HardwareDeviceConfiguration _hardwareDeviceConfiguration; 
        private NativeBuffer? _singleBuffer = null;
        private static readonly int _structSize = Marshal.SizeOf(typeof(TNet_ADHInfo));
        private ValveStatusEnum _valveStatus;
        private long _sampleSequence = 0;

        public BwControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _hardwareDeviceConfiguration = configuration;
            _highPriorityScheduler = CreateHighPriorityScheduler();

            // 创建批量采集流（热Observable - 始终运行）
            var acquisitionStream = CreateAcquisitionObservable().Publish();
            DataStream = acquisitionStream;

            // 从批量流展开为单条数据流
            // 使用 SelectMany 展开 + Publish().RefCount() 支持多订阅者
            IndividualDataStream = DataStream
                .SelectMany(batch => batch.Values.Select((value, index) => new DataPoint<TNet_ADHInfo>
                {
                    DeviceId = batch.DeviceId,
                    Timestamp = batch.ArrivalTicks + index, // 基于批次时间戳 + 索引区分
                    Value = value,
                    Unit = string.Empty,
                    DataQuality = DataQuality.Good
                }))
                .Publish()
                .RefCount();

            // ⚠️ 注意：采集在这里才真正启动
            _acquisitionSubscription = acquisitionStream.Connect();

            _statusSubscription = Observable.Interval(TimeSpan.FromSeconds(configuration.StatusInterval))
                .Subscribe(onNext: c =>
                {
                    uint t = 0;
                    var success = POPNetCtrl.NetCtrl01_ReadConectState(_deviceHandle, ref t);
                    var res = success switch
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
        private EventLoopScheduler CreateHighPriorityScheduler() =>
            new(ts => new Thread(ts)
            {
                Name = $"Controller_{_hardwareDeviceConfiguration.DeviceId}",
                IsBackground = true,
                Priority = ThreadPriority.Highest
            });

        private IObservable<SampleBatch<TNet_ADHInfo>> CreateAcquisitionObservable() =>
            Observable.Create<SampleBatch<TNet_ADHInfo>>(observer =>
            {
                return _highPriorityScheduler.SchedulePeriodic(TimeSpan.FromMilliseconds(2), () =>
                {
                    if (_deviceHandle == nint.Zero || Status != HardwareConnectionStatus.Connected)
                        return;

                    // 这一拍尽量读空（读到 null 就停）
                    while (true)
                    {
                        var frame = AcquireReading();
                        if (frame == null) break;
                        observer.OnNext(frame);
                    }
                });
            });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SampleBatch<TNet_ADHInfo>? AcquireReading()
        {
            uint count = 0;
            if (POPNetCtrl.NetCtrl01_GetAD_HDataCount(_deviceHandle, ref count) != AddressContanst.OP_SUCCESSFUL ||
                count == 0) return null;
            _singleBuffer ??= NativeBufferPool.Rent(128);
            var res = new SampleBatch<TNet_ADHInfo>
            {
                DeviceId = DeviceId,
                SampleCount = count,
                ArrivalTicks = Stopwatch.GetTimestamp(),
                SequenceStart = Interlocked.Add(ref _sampleSequence, count) - count
            };
            var tempValues = new TNet_ADHInfo[count];
            for (uint i = 0; i < count; i++)
            {
                if (POPNetCtrl.NetCtrl01_GetAD_HInfo(_deviceHandle, _singleBuffer.Ptr, (uint)_structSize) !=
                    AddressContanst.OP_SUCCESSFUL) return null;
                var tempValue = Marshal.PtrToStructure<TNet_ADHInfo>(_singleBuffer.Ptr);
                tempValues[i] = tempValue; 
            }
            res.Values = tempValues;
            return res;
        } 
        #endregion

        public bool ConnectToHardware()
        {
            var result = POPNetCtrl.NetCtrl01_ConnectToDev(_hardwareDeviceConfiguration.DeviceAddressId, ref _deviceHandle);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
#if DEBUG
                Debug.WriteLine($"✓ 设备连接成功，句柄: 0x{_hardwareDeviceConfiguration.DeviceAddressId:X}");
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

        public ValveStatusEnum GetValveStatus() => _valveStatus;

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

        public int SetStaticControlMode(StaticControlParams param) => POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, (uint)param.StaticLoadControl, param.Speed, param.TargetValue);

        public int SetValleyPeakFilterNum(int freq) => POPNetCtrl.NetCtrl01_bWriteAddr(_deviceHandle, AddressContanst.Addr_ValleyPeak_FilterNum, (byte)freq);

        public int SetDynamicControlMode(DynamicControlParams param) =>
            POPNetCtrl.NetCtrl01_Osci_SetWaveInfo(_hardwareDeviceConfiguration.DeviceAddressId, param.MeanValue,
                param.Amplitude,
                param.Frequency,
                (byte)param.WaveType,
                (byte)param.ControlMode,
                param.CompensateAmplitude,
                param.CompensationPhase,
                param.CycleCount,
                (int)param.CtrlOpt);

        public int SetDynamicStopControl(int tmpActMode, int tmpHaltState) => POPNetCtrl.NetCtrl01_Osci_SetHaltState(_hardwareDeviceConfiguration.DeviceAddressId, (byte)tmpActMode, (byte)tmpHaltState);

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
            if (_singleBuffer != null)
            {
                NativeBufferPool.Return(_singleBuffer);
                _singleBuffer = null;
            }
        }
        public override void Dispose()
        {
            _acquisitionSubscription?.Dispose();
            _highPriorityScheduler?.Dispose(); 
            CleanupResources();
            base.Dispose();  
        } 
    }
}
