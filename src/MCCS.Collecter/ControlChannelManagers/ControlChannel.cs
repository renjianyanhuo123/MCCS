using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.ControllerManagers.Entities;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.SignalManagers;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControlChannelManagers
{
    public class ControlChannel : IControlChannel
    { 
        private readonly IControllerManager _controllerManager;
        private readonly ISignalManager _signalManager; 
        private readonly IController _controller;

        private ControlChannel(ControlChannelConfiguration configuration)
        { 
            Configuration = configuration;
            ChannelId = configuration.ChannelId;
            ValveStatus = ValveStatusEnum.Closed; 
            _controllerManager = null!;
            _signalManager = null!;
            _controller = null!;
        }

        public ControlChannel(ControlChannelConfiguration configuration, IControllerManager controllerManager, ISignalManager signalManager) : this(configuration)
        {
            _controllerManager = controllerManager;
            _signalManager = signalManager;
            _controller = _controllerManager.GetControllerInfo(configuration.ControllerId);
            ControlState = _controller.GetControlState();
        }

        
        public ValveStatusEnum ValveStatus { get; private set; }
         
        public bool IsDynamicVibration { get; private set; } = false;
        
        public SystemControlState ControlState { get; private set; }

        public ControlChannelConfiguration Configuration { get; }

        public long ChannelId { get; }
         
        public bool OperationValve(bool isOpen)
        {
            if ((isOpen && ValveStatus == ValveStatusEnum.Opened) || (!isOpen && ValveStatus == ValveStatusEnum.Closed)) return true; 
            var res = _controller.SetValveStatus(isOpen);
            if (res == AddressContanst.OP_SUCCESSFUL)
            {
                ValveStatus = isOpen ? ValveStatusEnum.Opened : ValveStatusEnum.Closed;
                return true;
            }
            return false;
        } 

        public DeviceCommandContext ManualControl(float speed)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId,
                IsValid = false
            };
            if (speed is > 1.0f or < -1.0f)
            {
                context.Errmesage = "速度值超出范围!";
                return context;
            }   
            if (ControlState != SystemControlState.Static)
            {
                var success = _controller.SetControlState(SystemControlState.Static);
                if (!success)
                {
                    context.Errmesage = "信号设置静态模式失败";
                    return context;
                }
            }
            var setCtrlModeResult = _controller.SetStaticControlMode(new StaticControlParams()
            {
                StaticLoadControl = StaticLoadControlEnum.CTRLMODE_OPEN,
                Speed = speed, 
                TargetValue = 0
            });
            ControlState = SystemControlState.OpenLoop;
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
         
        public DeviceCommandContext StaticControl(StaticControlParams controlParam)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId,
                IsValid = false
            };   

            var controlState = _controller.GetControlState();
            if (controlState != SystemControlState.Static)
            {
                var success = _controller.SetControlState(SystemControlState.Static);
                if (!success)
                {
                    context.Errmesage = "信号设置静态模式失败";
                    return context;
                }
            }
            ControlState = SystemControlState.Static;
            var setCtrlModeResult = _controller.SetStaticControlMode(controlParam);
            if (setCtrlModeResult == AddressContanst.OP_SUCCESSFUL)
            {
                // 暂时没有做监测
                context.IsValid = true;
                IsDynamicVibration = false;
                context.ControlMode = SystemControlState.Static;
            }  
            return context;
        }
         
        public DeviceCommandContext DynamicControl(DynamicControlParams dynamicControlParam)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId,
                IsValid = false
            };  
            var setValleyPeakFilterNum = _controller.SetValleyPeakFilterNum(dynamicControlParam.ValleyPeakFilterNum);
            if (setValleyPeakFilterNum != AddressContanst.OP_SUCCESSFUL)
            {
                context.Errmesage = "发送超限次数指令失败";
                return context;
            } 
            if (ControlState != SystemControlState.Dynamic)
            {
                var success = _controller.SetControlState(SystemControlState.Dynamic);
                if (!success)
                {
                    context.Errmesage = "信号设置动态模式失败";
                    return context;
                }
            }
            ControlState = SystemControlState.Dynamic;
            var setDynamicControlResult = _controller.SetDynamicControlMode(dynamicControlParam);
            if (setDynamicControlResult != AddressContanst.OP_SUCCESSFUL)
            {
                context.Errmesage = "设置动态控制命令失败";
                return context;
            } 
            IsDynamicVibration = true;
            context.IsValid = true;
            context.ControlMode = SystemControlState.Static;
            return context;
        }
         
        public DeviceCommandContext StopControl()
        {
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId,
                IsValid = false
            };
            if (IsDynamicVibration && ControlState == SystemControlState.Dynamic)
            { 
                // 动态控制暂停，暂停到当前位置
                var res = _controller.SetDynamicStopControl(1, 1);
                if (res != AddressContanst.OP_SUCCESSFUL)
                {
                    context.Errmesage = "动态模式停止失败";
                    return context;
                }
            }
            else
            {
                // 保持当前位置暂停
                _controller.SetStaticControlMode(new StaticControlParams
                {
                    StaticLoadControl = StaticLoadControlEnum.CTRLMODE_HLoadS,
                    Speed = 0,
                    TargetValue = 0
                });
            }
            context.IsValid = true; 
            return context;
        }
        /// <summary>
        /// 设置控制通道清零
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public DeviceCommandContext SetControlChannelTare(int controlType)
        {
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId,
                IsValid = false
            };
            var success = _controller.SetSignalTare(controlType);
            switch (success)
            {
                case AddressContanst.OP_SUCCESSFUL:
                    context.IsValid = true;
                    break;
                case 10:
                    context.Errmesage = "控制器不处于静态状态";
                    break;
                case 20:
                    context.Errmesage = "控制器正处于静态运行状态";
                    break;
                default:
                    context.Errmesage = "其他错误类型";
                    break;
            }

            return context;
        }
    }
}
