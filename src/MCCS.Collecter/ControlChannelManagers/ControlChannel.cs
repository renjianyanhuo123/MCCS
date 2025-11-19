using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.ControllerManagers.Entities;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.SignalManagers;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControlChannelManagers
{
    public class ControlChannel
    { 
        private readonly IControllerManager _controllerManager;
        private readonly ISignalManager _signalManager;
        private readonly bool _isMock = false;
        private readonly ControlChannelSignalConfiguration _outPutSignalConfiguration;

        private ControlChannel(ControlChannelConfiguration configuration, bool isMock)
        {
            _isMock = isMock;
            Configuration = configuration;
            ChannelId = configuration.ChannelId;
            ValveStatus = ValveStatusEnum.Closed; 
        }

        public ControlChannel(ControlChannelConfiguration configuration, IControllerManager controllerManager, ISignalManager signalManager, bool isMock = false) : this(configuration, isMock)
        {
            _controllerManager = controllerManager;
            _signalManager = signalManager;
            _outPutSignalConfiguration = Configuration.SignalConfiguration.FirstOrDefault(s => s.SignalType == ControlChannelSignalTypeEnum.Output) ?? throw new ArgumentNullException("没有对应的输出信号接口");
            ControlState = _signalManager.GetControlStateBySignalId(_outPutSignalConfiguration.SignalId);
        }

        /// <summary>
        /// 阀门状态
        /// </summary>
        public ValveStatusEnum ValveStatus { get; private set; }

        /// <summary>
        /// 控制通道是否处于起振状态
        /// </summary>
        public bool IsDynamicVibration { get; private set; } = false;
        /// <summary>
        /// 控制模式
        /// </summary>
        public SystemControlState ControlState { get; private set; }

        public ControlChannelConfiguration Configuration { get; }

        public long ChannelId { get; }
        
        /// <summary>
        /// 操作控制阀门
        /// </summary>
        /// <param name="isOpen"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool OperationValve(bool isOpen)
        {
            if ((isOpen && ValveStatus == ValveStatusEnum.Opened) || (!isOpen && ValveStatus == ValveStatusEnum.Closed)) return true;
            var outputSignalId = Configuration.SignalConfiguration.FirstOrDefault(c => c.SignalType == ControlChannelSignalTypeEnum.Output)?.SignalId;
            if (outputSignalId == null) throw new ArgumentNullException("没有找到对应的输出信号");
            if (_isMock)
            {
                ValveStatus = isOpen ? ValveStatusEnum.Opened : ValveStatusEnum.Closed;
                return true;
            }
            var res = _signalManager.SetValveStatus((long)outputSignalId, isOpen);
            if (res == AddressContanst.OP_SUCCESSFUL)
            {
                ValveStatus = isOpen ? ValveStatusEnum.Opened : ValveStatusEnum.Closed;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 手动控制
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
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
                var success = _signalManager.SetControlStateBySignalId(_outPutSignalConfiguration.SignalId, SystemControlState.Static);
                if (!success)
                {
                    context.Errmesage = "信号设置静态模式失败";
                    return context;
                }
            }
            var setCtrlModeResult = _signalManager.SetStaticControlMode(_outPutSignalConfiguration.SignalId, StaticLoadControlEnum.CTRLMODE_LoadS, speed, 0);
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

        /// <summary>
        /// 静态控制
        /// </summary>
        /// <param name="controlParam"></param>
        /// <returns></returns>
        public DeviceCommandContext StaticControl(StaticControlParams controlParam)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId,
                IsValid = false
            };  
            // 模拟操作
            if (_isMock)
            {
                var controller = _controllerManager.GetControllerInfo(_outPutSignalConfiguration.BelongControllerId);
                if (controller is MockControllerHardwareDevice mockController)
                {
                    mockController.MockStaticControl(controlParam);
                    context.IsValid = true;
                    context.ControlMode = SystemControlState.Static;
                }
                else
                {
                    context.Errmesage = "没有找到对应的模拟控制器";
                }
                return context;
            }

            var controlState = _signalManager.GetControlStateBySignalId(_outPutSignalConfiguration.SignalId);
            if (controlState != SystemControlState.Static)
            {
                var success = _signalManager.SetControlStateBySignalId(_outPutSignalConfiguration.SignalId, SystemControlState.Static);
                if (!success)
                {
                    context.Errmesage = "信号设置静态模式失败";
                    return context;
                }
            }
            var setCtrlModeResult = _signalManager.SetStaticControlMode(_outPutSignalConfiguration.SignalId, controlParam.StaticLoadControl, controlParam.Speed / 60.0f, controlParam.TargetValue);
            if (setCtrlModeResult == AddressContanst.OP_SUCCESSFUL)
            {
                // 暂时没有做监测
                context.IsValid = true;
                context.ControlMode = SystemControlState.Static;
            }
            return context;
        }

        /// <summary>
        /// 动态控制
        /// </summary>
        /// <param name="dynamicControlParam"></param>
        /// <returns></returns>
        public DeviceCommandContext DynamicControl(DynamicControlParams dynamicControlParam)
        {
            // 创建或获取设备上下文
            var context = new DeviceCommandContext
            {
                ControlChannelId = ChannelId,
                IsValid = false
            };  
            var setValleyPeakFilterNum = _signalManager.SetValleyPeakFilterNum(_outPutSignalConfiguration.SignalId,
                dynamicControlParam.ValleyPeakFilterNum);
            if (setValleyPeakFilterNum != AddressContanst.OP_SUCCESSFUL)
            {
                context.Errmesage = "发送超限次数指令失败";
                return context;
            } 
            if (ControlState != SystemControlState.Dynamic)
            {
                var success = _signalManager.SetControlStateBySignalId(_outPutSignalConfiguration.SignalId, SystemControlState.Dynamic);
                if (!success)
                {
                    context.Errmesage = "信号设置动态模式失败";
                    return context;
                }
            }
            var setDynamicControlResult = _signalManager.SetDynamicControlMode(_outPutSignalConfiguration.SignalId,
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

        /// <summary>
        /// 停止控制
        /// </summary>
        /// <returns></returns>
        public DeviceCommandContext StopControl()
        {
            if (IsDynamicVibration)
            { 
                // 动态控制暂停，暂停到当前位置
            }
            else
            {
                // 保持当前位置暂停
            }
        }
    }
}
