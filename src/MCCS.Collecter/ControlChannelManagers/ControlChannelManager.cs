using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.ControllerManagers.Entities;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.SignalManagers;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControlChannelManagers
{
    public sealed class ControlChannelManager : IControlChannelManager 
    {
        private readonly Dictionary<long, ControlChannel> _channelDics = [];
        private readonly IControllerManager _controllerManager;
        private readonly ISignalManager _signalManager;
        private bool _isMock = false;

        public ControlChannelManager(IControllerManager controllerManager,
            ISignalManager signalManager)
        {
            _controllerManager = controllerManager;
            _signalManager = signalManager;
        }

        public void Initialization(IEnumerable<ControlChannelConfiguration> configurations, bool isMock = false)
        {
            _isMock = isMock;
            foreach (var configuration in configurations)
            {
                var channel = new ControlChannel(configuration, _controllerManager, _signalManager);
                channel.OperationValve(false); // 默认关闭阀门
                _channelDics.TryAdd(configuration.ChannelId, channel);
            } 
        }

        public ControlChannel GetControlChannel(long channelId)
        {
            return _channelDics[channelId]; 
        }

        public bool OperationValveStatus(long channelId, bool isOpen)
        { 
            return _channelDics[channelId].OperationValve(isOpen);
        }

        public bool AddControlChannel(ControlChannelConfiguration configuration)
        {
            return _channelDics.TryAdd(configuration.ChannelId, new ControlChannel(configuration, _controllerManager, _signalManager));
        }

        public bool RemoveControlChannel(long channelId)
        {
            return _channelDics.Remove(channelId, out var tempChannel);
        } 

        public DeviceCommandContext DynamicControl(long channelId, DynamicControlParams dynamicControlParam)
        {
            return _channelDics[channelId].DynamicControl(dynamicControlParam);
        }

        public DeviceCommandContext ManualControl(long channelId, float speed)
        {
            return _channelDics[channelId].ManualControl(speed);
        }

        public DeviceCommandContext StaticControl(long channelId, StaticControlParams controlParam)
        {  
            return _channelDics[channelId].StaticControl(controlParam);
        }

        public DeviceCommandContext StopControl(long channelId)
        {
            return _channelDics[channelId].StopControl();
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
