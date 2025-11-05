using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.DllNative.Models;
using MCCS.Collecter.HardwareDevices;
using MCCS.Infrastructure.TestModels;

namespace MCCS.Collecter.ControllerManagers.Entities
{
    public sealed class BwControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        private readonly IDisposable _acquisitionSubscription;
        private readonly EventLoopScheduler _highPriorityScheduler; 
        private readonly HardwareDeviceConfiguration _hardwareDeviceConfiguration;
        private nint _singleBuffer = nint.Zero;
        private static readonly int _structSize = Marshal.SizeOf(typeof(TNet_ADHInfo));

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

        //#region Control Method 
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

        //public override DeviceCommandContext ManualControl(long deviceId, float outValue)
        //{
        //    // 创建或获取设备上下文
        //    var context = _deviceContexts.GetOrAdd(deviceId, new DeviceCommandContext
        //    {
        //        DeviceId = deviceId,
        //        IsValid = false
        //    });
        //    if (Status != HardwareConnectionStatus.Connected) return context;
        //    if (ControlState != SystemControlState.Static)
        //    {
        //        var setCtrlstateResult = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)SystemControlState.Static);
        //        if (setCtrlstateResult != AddressContanst.OP_SUCCESSFUL) return context;
        //        ControlState = SystemControlState.Static;
        //    }
        //    var setCtrlModeResult = POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, (uint)StaticLoadControlEnum.CTRLMODE_LoadS, outValue, 0);
        //    if (setCtrlModeResult == AddressContanst.OP_SUCCESSFUL)
        //    {
        //        context.IsValid = true;
        //        context.ControlMode = SystemControlState.OpenLoop;
        //        context.CurrentStatus = CommandExecuteStatusEnum.Executing;
        //    }
        //    return context;
        //}

        //public override DeviceCommandContext StaticControl(StaticControlParams controlParams)
        //{
        //    // 创建或获取设备上下文
        //    var context = _deviceContexts.GetOrAdd(controlParams.DeviceId, new DeviceCommandContext
        //    {
        //        DeviceId = controlParams.DeviceId,
        //        IsValid = false
        //    });
        //    if (Status != HardwareConnectionStatus.Connected) return context;
        //    if (ControlState != SystemControlState.Static)
        //    {
        //        var setCtrlstateResult = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)SystemControlState.Static);
        //        if (setCtrlstateResult != AddressContanst.OP_SUCCESSFUL) return context;
        //        ControlState = SystemControlState.Static;
        //    }
        //    var result = POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, (uint)controlParams.StaticLoadControl, controlParams.Speed, controlParams.TargetValue);

        //    if (result == AddressContanst.OP_SUCCESSFUL)
        //    {
        //        context.IsValid = true;
        //        context.ControlMode = SystemControlState.Static;
        //        context.CommandSubscribetion = IndividualDataStream.Buffer(context.BufferSize, context.BufferSize)
        //            .Where(buffer => buffer.Count == context.BufferSize)
        //            .ObserveOn(TaskPoolScheduler.Default)
        //            .Subscribe(datas =>
        //            {
        //                foreach (var data in datas)
        //                {
        //                    // 根据设备寻找所有的signal对象; 可能一个设备上有多个Signal
        //                    //if (signal.Value != null)
        //                    //{
        //                    //    var index = signal.Value.SignalAddressIndex;
        //                    //    CheckCompletionCondition(
        //                    //        controlParams.StaticLoadControl,
        //                    //        controlParams.TargetValue,
        //                    //        data.Net_AD_S[index],
        //                    //        );
        //                    //}


        //                }
        //            });
        //    }
        //    return context;
        //}

        //public override DeviceCommandContext DynamicControl(DynamicControlParams controlParams)
        //{
        //    // 创建或获取设备上下文
        //    var context = _deviceContexts.GetOrAdd(controlParams.DeviceId, new DeviceCommandContext
        //    {
        //        DeviceId = controlParams.DeviceId,
        //        IsValid = false
        //    });
        //    if (Status != HardwareConnectionStatus.Connected) return context;
        //    if (ControlState != SystemControlState.Dynamic)
        //    {
        //        var setCtrlstateResult = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)SystemControlState.Dynamic);
        //        if (setCtrlstateResult != AddressContanst.OP_SUCCESSFUL) return context;
        //        ControlState = SystemControlState.Dynamic;
        //    }
        //    var result = POPNetCtrl.NetCtrl01_Osci_SetWaveInfo((int)DeviceId,
        //        controlParams.MeanValue,
        //        controlParams.Amplitude,
        //        controlParams.Frequency,
        //        (byte)controlParams.WaveType,
        //        (byte)controlParams.ControlMode,
        //        controlParams.CompensateAmplitude,
        //        controlParams.CompensationPhase,
        //        controlParams.CycleCount,
        //        controlParams.IsAdjustedMedian ? 0 : 1
        //        );
        //    if (result == AddressContanst.OP_SUCCESSFUL)
        //    {
        //        context.IsValid = true;
        //        context.ControlMode = SystemControlState.Dynamic;
        //        context.CurrentStatus = CommandExecuteStatusEnum.Executing;
        //    }
        //    return context;
        //}

        //#endregion

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
                        var errorReading = new DataPoint<List<BatchCollectItemModel>>
                        {
                            Value = [],
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
        private DataPoint<List<BatchCollectItemModel>> AcquireReading()
        {
            uint count = 0;
            if (POPNetCtrl.NetCtrl01_GetAD_HDataCount(_deviceHandle, ref count) != AddressContanst.OP_SUCCESSFUL || count == 0)
                return CreateBadDataPoint();
            if (_singleBuffer == nint.Zero)
                _singleBuffer = BufferPool.Rent();
            var results = new List<BatchCollectItemModel>((int)count);
            for (uint i = 0; i < count; i++)
            {
                if (POPNetCtrl.NetCtrl01_GetAD_HInfo(_deviceHandle, _singleBuffer, (uint)_structSize) != AddressContanst.OP_SUCCESSFUL)
                    return CreateBadDataPoint();
                var tempValue = Marshal.PtrToStructure<TNet_ADHInfo>(_singleBuffer);
                results.Add(StructDataToCollectModel(tempValue));
            }

            return new DataPoint<List<BatchCollectItemModel>>
            {
                DeviceId = DeviceId,
                Value = results,
                Timestamp = Stopwatch.GetTimestamp(),
                DataQuality = DataQuality.Good
            };
        }

        private static DataPoint<List<BatchCollectItemModel>> CreateBadDataPoint() => new()
        {
            Value = [],
            Timestamp = Stopwatch.GetTimestamp(),
            DataQuality = DataQuality.Bad
        };

        ///// <summary>
        ///// 检查完成条件
        ///// </summary>
        ///// <param name="mode"></param>
        ///// <param name="targetValue"></param>
        ///// <param name="currentDisplacement"></param>
        ///// <param name="currentForce"></param>
        ///// <param name="posE"></param>
        ///// <param name="ctrlOutput"></param>
        ///// <param name="stableCount"></param>
        ///// <param name="requiredStableCount"></param>
        ///// <returns></returns>
        //private bool CheckCompletionCondition(
        //    StaticLoadControlEnum mode,
        //    double targetValue,
        //    double currentDisplacement,
        //    double currentForce,
        //    double posE,
        //    double ctrlOutput,
        //    ref int stableCount,
        //    int requiredStableCount)
        //{
        //    var condition1 = false;
        //    var condition2 = false;
        //    var condition3 = false;

        //    switch (mode)
        //    {
        //        case StaticLoadControlEnum.CTRLMODE_LoadS: // 位移控制
        //            condition1 = Math.Abs(currentDisplacement - targetValue) < _hardwareDeviceConfiguration.CompletionConfig?.DisplacementTolerance;
        //            condition2 = Math.Abs(posE) < _hardwareDeviceConfiguration.CompletionConfig?.PosErrorThreshold;
        //            condition3 = Math.Abs(ctrlOutput) < _hardwareDeviceConfiguration.CompletionConfig?.ControlOutputThreshold;
        //            break;

        //        case StaticLoadControlEnum.CTRLMODE_LoadN: // 力控制
        //            condition1 = Math.Abs(currentForce - targetValue) < _hardwareDeviceConfiguration.CompletionConfig?.ForceTolerance;
        //            condition2 = Math.Abs(posE) < _hardwareDeviceConfiguration.CompletionConfig?.ForceTolerance;
        //            condition3 = Math.Abs(ctrlOutput) < _hardwareDeviceConfiguration.CompletionConfig?.ControlOutputThreshold;
        //            break;

        //        case StaticLoadControlEnum.CTRLMODE_LoadSVNP: // 位移控制(力判断停止)
        //            // 先检查力是否到达（提前停止条件）
        //            var forceReached = Math.Abs(currentForce - targetValue) < _hardwareDeviceConfiguration.CompletionConfig?.ForceTolerance;
        //            if (forceReached)
        //            {
        //                condition1 = condition2 = condition3 = true;
        //            }
        //            else
        //            {
        //                // 检查位移是否到达
        //                condition1 = Math.Abs(currentDisplacement - targetValue) < _hardwareDeviceConfiguration.CompletionConfig?.DisplacementTolerance;
        //                condition2 = Math.Abs(posE) < _hardwareDeviceConfiguration.CompletionConfig?.PosErrorThreshold;
        //                condition3 = Math.Abs(ctrlOutput) < _hardwareDeviceConfiguration.CompletionConfig?.ControlOutputThreshold;
        //            }
        //            break;

        //        case StaticLoadControlEnum.CTRLMODE_LoadNVSP: // 力控制(位移判断停止)
        //            // 先检查位移是否到达（提前停止条件）
        //            var displacementReached = Math.Abs(currentDisplacement - targetValue) < _hardwareDeviceConfiguration.CompletionConfig?.DisplacementTolerance;
        //            if (displacementReached)
        //            {
        //                condition1 = condition2 = condition3 = true;
        //            }
        //            else
        //            {
        //                // 检查力是否到达
        //                condition1 = Math.Abs(currentForce - targetValue) < _hardwareDeviceConfiguration.CompletionConfig?.ForceTolerance;
        //                condition2 = Math.Abs(posE) < _hardwareDeviceConfiguration.CompletionConfig?.ForceTolerance;
        //                condition3 = Math.Abs(ctrlOutput) < _hardwareDeviceConfiguration.CompletionConfig?.ControlOutputThreshold;
        //            }
        //            break;
        //    }

        //    if (condition1 && condition2 && condition3)
        //    {
        //        stableCount++;
        //        return stableCount >= requiredStableCount;
        //    }

        //    return false;
        //}
        #endregion
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
