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
                controlChannel.Configuration.SignalConfiguratetion.FirstOrDefault(s =>
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
        }

        public void Dispose()
        {
            // TODO release managed resources here
        } 
    }
}
