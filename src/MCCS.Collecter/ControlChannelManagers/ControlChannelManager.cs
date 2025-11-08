using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.SignalManagers;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControlChannelManagers
{
    public sealed class ControlChannelManager : IControlChannelManager 
    {
        private readonly Dictionary<long, ControlChannel> _channelDics;
        private readonly IControllerManager _controllerManager;
        private readonly ISignalManager _signalManager;

        public ControlChannelManager(IControllerManager controllerManager,
            ISignalManager signalManager)
        {
            _controllerManager = controllerManager;
            _signalManager = signalManager;
        }

        public void Initialization(IEnumerable<ControlChannelConfiguration> configurations)
        {
            foreach (var configuration in configurations)
            {
                _channelDics.TryAdd(configuration.ChannelId, new ControlChannel(configuration));
            } 
        }

        public bool AddControlChannel(ControlChannelConfiguration configuration)
        {
            return _channelDics.TryAdd(configuration.ChannelId, new ControlChannel(configuration));
        }

        public bool RemoveControlChannel(long channelId)
        {
            return _channelDics.Remove(channelId, out var tempChannel);
        } 
        public DeviceCommandContext DynamicControl(long channelId, DynamicControlParams dynamicControlParam)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = channelId,
                IsValid = false
            };
            var controlChannel = _channelDics.GetValueOrDefault(channelId);
            if (controlChannel == null)
            {
                context.Errmesage = "通道未添加！";
                return context;
            }
            var outPutSignalConfiguration =
                controlChannel.Configuration.SignalConfiguration.FirstOrDefault(s =>
                    s.SignalType == ControlChannelSignalTypeEnum.Output);
            if (outPutSignalConfiguration == null)
            {
                context.Errmesage = "没有完整的信号接口";
                return context;
            }

            var setValleyPeakFilterNum = _signalManager.SetValleyPeakFilterNum(outPutSignalConfiguration.SignalId,
                dynamicControlParam.ValleyPeakFilterNum);
            if (setValleyPeakFilterNum != AddressContanst.OP_SUCCESSFUL)
            {
                context.Errmesage = "发送超限次数指令失败";
                return context;
            } 
            var controlState = _signalManager.GetControlStateBySignalId(outPutSignalConfiguration.SignalId);
            if (controlState != SystemControlState.Dynamic)
            {
                var success = _signalManager.SetControlStateBySignalId(outPutSignalConfiguration.SignalId, SystemControlState.Dynamic);
                if (!success)
                {
                    context.Errmesage = "信号设置动态模式失败";
                    return context;
                }
            }
            var setDynamicControlResult = _signalManager.SetDynamicControlMode(outPutSignalConfiguration.SignalId,
                dynamicControlParam.MeanValue,
                dynamicControlParam.Amplitude,
                dynamicControlParam.Frequency,
                (byte)dynamicControlParam.WaveType,
                (byte)dynamicControlParam.ControlMode,
                dynamicControlParam.CompensateAmplitude,
                dynamicControlParam.CompensationPhase,
                dynamicControlParam.CycleCount,
                (int)dynamicControlParam.CtrlOpt);
            if (setDynamicControlResult != AddressContanst.OP_SUCCESSFUL)
            {
                context.Errmesage = "设置动态控制命令失败";
                return context;
            }
            context.IsValid = true;
            context.ControlMode = SystemControlState.Static;
            return context;
        }

        public DeviceCommandContext ManualControl(long channelId, float speed)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = channelId,
                IsValid = false
            };
            if (speed is > 1.0f or < -1.0f)
            {
                context.Errmesage = "速度值超出范围!";
                return context;
            }

            var controlChannel = _channelDics.GetValueOrDefault(channelId);
            if (controlChannel == null)
            {
                context.Errmesage = "通道未添加！";
                return context;
            }  
            var outPutSignalConfiguration =
                controlChannel.Configuration.SignalConfiguration.FirstOrDefault(s =>
                    s.SignalType == ControlChannelSignalTypeEnum.Output);
            if (outPutSignalConfiguration == null)
            {
                context.Errmesage = "没有对应的输出信号接口";
                return context;
            }
            var controlState = _signalManager.GetControlStateBySignalId(outPutSignalConfiguration.SignalId);
            if (controlState != SystemControlState.Static)
            {
                var success = _signalManager.SetControlStateBySignalId(outPutSignalConfiguration.SignalId, SystemControlState.Static);
                if (!success)
                {
                    context.Errmesage = "信号设置静态模式失败";
                    return context;
                }
            }
            var setCtrlModeResult = _signalManager.SetStaticControlMode(outPutSignalConfiguration.SignalId, StaticLoadControlEnum.CTRLMODE_LoadS, speed, 0);
            if (setCtrlModeResult != AddressContanst.OP_SUCCESSFUL)
            {
                context.Errmesage = "命令发送失败";
                return context;
            }
            context.IsValid = true;
            context.ControlMode = SystemControlState.OpenLoop;
            context.CurrentStatus = CommandExecuteStatusEnum.Executing;
            return context;
        }

        public DeviceCommandContext StaticControl(long channelId, StaticControlParams controlParam)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = channelId,
                IsValid = false
            };
            var controlChannel = _channelDics.GetValueOrDefault(channelId);
            if (controlChannel == null)
            {
                context.Errmesage = "通道未添加！";
                return context;
            } 
            var outPutSignalConfiguration =
                controlChannel.Configuration.SignalConfiguration.FirstOrDefault(s =>
                    s.SignalType == ControlChannelSignalTypeEnum.Output); 
            if (outPutSignalConfiguration == null )
            {
                context.Errmesage = "没有完整的信号接口";
                return context;
            }
            var controlState = _signalManager.GetControlStateBySignalId(outPutSignalConfiguration.SignalId);
            if (controlState != SystemControlState.Static)
            {
                var success = _signalManager.SetControlStateBySignalId(outPutSignalConfiguration.SignalId, SystemControlState.Static);
                if (!success)
                {
                    context.Errmesage = "信号设置静态模式失败";
                    return context;
                }
            }
            var setCtrlModeResult = _signalManager.SetStaticControlMode(outPutSignalConfiguration.SignalId, controlParam.StaticLoadControl, controlParam.Speed / 60.0f, controlParam.TargetValue);
            if (setCtrlModeResult == AddressContanst.OP_SUCCESSFUL)
            {
                // 暂时没有做监测
                context.IsValid = true;
                context.ControlMode = SystemControlState.Static; 
            }
            return context;
        }

        /// <summary>
        /// 检查完成条件
        /// </summary>
        /// <param name="completionConfiguration"></param>
        /// <param name="mode"></param>
        /// <param name="targetValue"></param>
        /// <param name="currentDisplacement"></param>
        /// <param name="currentForce"></param>
        /// <param name="posE"></param>
        /// <param name="ctrlOutput"></param>
        /// <param name="stableCount"></param>
        /// <param name="requiredStableCount"></param>
        /// <returns></returns>
        [Obsolete]
        private static bool CheckCompletionCondition(
            ControlCompletionConfiguration completionConfiguration,
            StaticLoadControlEnum mode,
            double targetValue,
            double currentDisplacement,
            double currentForce,
            double posE,
            double ctrlOutput,
            ref int stableCount,
            int requiredStableCount)
        {
            var condition1 = false;
            var condition2 = false;
            var condition3 = false;

            switch (mode)
            {
                case StaticLoadControlEnum.CTRLMODE_LoadS: // 位移控制
                    condition1 = Math.Abs(currentDisplacement - targetValue) < completionConfiguration.DisplacementTolerance;
                    condition2 = Math.Abs(posE) < completionConfiguration.PosErrorThreshold;
                    condition3 = Math.Abs(ctrlOutput) < completionConfiguration.ControlOutputThreshold;
                    break;

                case StaticLoadControlEnum.CTRLMODE_LoadN: // 力控制
                    condition1 = Math.Abs(currentForce - targetValue) < completionConfiguration.ForceTolerance;
                    condition2 = Math.Abs(posE) < completionConfiguration.ForceTolerance;
                    condition3 = Math.Abs(ctrlOutput) < completionConfiguration.ControlOutputThreshold;
                    break;

                case StaticLoadControlEnum.CTRLMODE_LoadSVNP: // 位移控制(力判断停止)
                    // 先检查力是否到达（提前停止条件）
                    var forceReached = Math.Abs(currentForce - targetValue) < completionConfiguration.ForceTolerance;
                    if (forceReached)
                    {
                        condition1 = condition2 = condition3 = true;
                    }
                    else
                    {
                        // 检查位移是否到达
                        condition1 = Math.Abs(currentDisplacement - targetValue) < completionConfiguration.DisplacementTolerance;
                        condition2 = Math.Abs(posE) < completionConfiguration.PosErrorThreshold;
                        condition3 = Math.Abs(ctrlOutput) < completionConfiguration.ControlOutputThreshold;
                    }
                    break;

                case StaticLoadControlEnum.CTRLMODE_LoadNVSP: // 力控制(位移判断停止)
                    // 先检查位移是否到达（提前停止条件）
                    var displacementReached = Math.Abs(currentDisplacement - targetValue) < completionConfiguration.DisplacementTolerance;
                    if (displacementReached)
                    {
                        condition1 = condition2 = condition3 = true;
                    }
                    else
                    {
                        // 检查力是否到达
                        condition1 = Math.Abs(currentForce - targetValue) < completionConfiguration.ForceTolerance;
                        condition2 = Math.Abs(posE) < completionConfiguration.ForceTolerance;
                        condition3 = Math.Abs(ctrlOutput) < completionConfiguration.ControlOutputThreshold;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            if (condition1 && condition2 && condition3)
            {
                stableCount++;
                return stableCount >= requiredStableCount;
            }

            return false;
        }

        public void Dispose()
        {
            // TODO release managed resources here
        } 
    }
}
