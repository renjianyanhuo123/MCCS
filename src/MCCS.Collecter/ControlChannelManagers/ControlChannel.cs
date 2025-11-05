using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.DllNative;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControlChannelManagers
{
    internal class ControlChannel
    {
        private readonly ControlChannelConfiguration _configuration;
        private readonly IControllerManager _controllerManager;


        public ControlChannel(ControlChannelConfiguration configuration)
        {
            _configuration = configuration;
            ChannelId = configuration.ChannelId;
        }

        public ControlChannel(ControlChannelConfiguration configuration, IControllerManager controllerManager) : this(configuration)
        {
            _controllerManager = controllerManager;
        }

        public long ChannelId { get; }

        public DeviceCommandContext DynamicControl(long channelId, DynamicControlParams dynamicControlParam)
        {
            throw new NotImplementedException();
        }
         
        public DeviceCommandContext ManualControl(long channelId, float speed)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId, 
                IsValid = false
            };
            var state = _controllerManager.GetControlStateBySignalId();
            if (ControlState != SystemControlState.Static)
            {
                var setCtrlstateResult = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)SystemControlState.Static);
                if (setCtrlstateResult != AddressContanst.OP_SUCCESSFUL) return context;
                ControlState = SystemControlState.Static;
            }
            var setCtrlModeResult = POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, (uint)StaticLoadControlEnum.CTRLMODE_LoadS, outValue, 0);
            if (setCtrlModeResult == AddressContanst.OP_SUCCESSFUL)
            {
                context.IsValid = true;
                context.ControlMode = SystemControlState.OpenLoop;
                context.CurrentStatus = CommandExecuteStatusEnum.Executing;
            }
            return context;
        }

        public DeviceCommandContext StaticControl(long channelId, StaticControlParams controlParam)
        {
            throw new NotImplementedException();
        }
    }
}
