using MCCS.Collecter.DllNative;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.HardwareDevices.BwController;
using MCCS.Infrastructure.Enums;
using MCCS.Infrastructure.Helper;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.CommandTracking;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.Services
{
    /// <summary>
    /// 控制器服务(务必保持单例)
    /// </summary>
    public sealed class ControllerService : IControllerService
    {
        private static volatile bool _isDllInitialized = false;
        private readonly List<ControllerHardwareDeviceBase> _controllers = [];
        private readonly ICommandTrackingService _commandTrackingService;
        private bool _isMock = false;

        public ControllerService(ICommandTrackingService commandTrackingService)
        {
            _commandTrackingService = commandTrackingService;
        }

        public bool InitializeDll(bool isMock = false)
        {
            if (_isDllInitialized || isMock) 
            {
                _isMock = true;
                return true;
            }
            if (!FileHelper.FileExists(AddressContanst.DllName)) throw new DllNotFoundException("DLL文件不存在");
            var result = POPNetCtrl.NetCtrl01_Init();
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                _isDllInitialized = true;
                return true;
            }
            throw new Exception($"DLL初始化失败,错误码:{result}"); 
        }
        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ControllerHardwareDeviceBase GetControllerInfo(long controllerId)
        {
            var controller = _controllers.FirstOrDefault(c => c.DeviceId == controllerId);
            if (controller == null) throw new ArgumentNullException("controllerId is Null!");
            return controller;
        }

        /// <summary>
        /// 操作阀门
        /// </summary>
        public bool OperationSigngleValve(long controllerId, bool isOpen)
        {
            return GetControllerInfo(controllerId).OperationValveState(isOpen);
        }
         
        /// <summary>
        /// 操作整个实验
        /// </summary>
        /// <param name="isStart"></param>
        /// <returns></returns>
        public bool OperationTest(bool isStart)
        {
            var temp = isStart ? 1u : 0u;
            return _controllers.Select(controller => controller.OperationTest(temp)).All(success => success);
        }

        /// <summary>
        /// 操作单个控制器的控制模式
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        public bool OperationControlMode(long controllerId, SystemControlState controlMode)
        {
            return GetControllerInfo(controllerId).OperationControlMode(controlMode);
        }

        /// <summary>
        /// 手动控制
        /// </summary>
        /// <param name="controllerId">控制器ID</param>
        /// <param name="deviceId">对应控制的作动器设备ID</param>
        /// <param name="speed">作动器位移的速度</param>
        /// <returns></returns>
        public bool ManualControl(long controllerId, long deviceId, float speed)
        {
            var controller = GetControllerInfo(controllerId);
            return controller.ManualControl(speed);
        }

        /// <summary>
        /// 静态控制
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="deviceId"></param>
        /// <param name="staticControlParam"></param>
        /// <returns></returns>
        public CommandRecord? StaticControl(long controllerId, long deviceId, StaticControlParams staticControlParam)
        {
            try
            {
                var controller = GetControllerInfo(controllerId);

                // 创建命令记录
                var commandRecord = _commandTrackingService.CreateCommand(
                    (int)controllerId,
                    (int)deviceId,
                    ControlMode.Static,
                    staticControlParam);

                // 发送命令
                _commandTrackingService.UpdateCommandStatus(commandRecord.CommandId, CommandExecuteStatusEnum.Executing);
                var success = controller.StaticControl(staticControlParam);

                if (success)
                {
                    // 命令发送成功，保持 Executing 状态
                    // 实际完成状态需要通过监听硬件反馈来更新
                    return commandRecord;
                }
                else
                {
                    // 命令发送失败
                    _commandTrackingService.UpdateCommandStatus(
                        commandRecord.CommandId,
                        CommandExecuteStatusEnum.Error,
                        "命令发送失败");
                    return commandRecord;
                }
            }
            catch (Exception ex)
            {
                // 发生异常，无法创建命令记录
                return null;
            }
        }

        /// <summary>
        /// 动态控制
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="deviceId"></param>
        /// <param name="dynamicControlParam"></param>
        /// <returns></returns>
        public CommandRecord? DynamicControl(long controllerId, long deviceId, DynamicControlParams dynamicControlParam)
        {
            try
            {
                var controller = GetControllerInfo(controllerId);

                // 创建命令记录
                var commandRecord = _commandTrackingService.CreateCommand(
                    (int)controllerId,
                    (int)deviceId,
                    ControlMode.Fatigue,
                    dynamicControlParam);

                // 发送命令
                _commandTrackingService.UpdateCommandStatus(commandRecord.CommandId, CommandExecuteStatusEnum.Executing);
                var success = controller.DynamicControl(dynamicControlParam);

                if (success)
                {
                    // 命令发送成功，保持 Executing 状态
                    // 实际完成状态需要通过监听硬件反馈来更新
                    return commandRecord;
                }
                else
                {
                    // 命令发送失败
                    _commandTrackingService.UpdateCommandStatus(
                        commandRecord.CommandId,
                        CommandExecuteStatusEnum.Error,
                        "命令发送失败");
                    return commandRecord;
                }
            }
            catch (Exception ex)
            {
                // 发生异常，无法创建命令记录
                return null;
            }
        }

        /// <summary>
        /// 创建控制器
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public bool CreateController(HardwareDeviceConfiguration configuration)
        {
            ControllerHardwareDeviceBase controller;
            if (_isMock)
            {
                controller = new MockControllerHardwareDevice(configuration);
            }
            else
            {
                controller = new BwControllerHardwareDevice(configuration);
            } 
            controller.ConnectToHardware();
            _controllers.Add(controller);
            return true;
        }

        /// <summary>
        /// 移除控制器
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public bool RemoveController(int deviceId)
        {
            var controller = _controllers.FirstOrDefault(c => c.DeviceId == deviceId);
            if (controller == null) return true;
            controller.DisconnectFromHardware();
            controller.Dispose();
            _controllers.Remove(controller);
            return true;
        }

        /// <summary>
        /// 开启所有控制器的数据采集
        /// </summary>
        public void StartAllControllers()
        {
            foreach (var controller in _controllers)
            {
                if (controller.Status != HardwareConnectionStatus.Connected)
                {
                    controller.ConnectToHardware();
                }
                controller.StartDataAcquisition();
            }
        }

        /// <summary>
        /// 停止所有控制器的数据采集并断开连接
        /// </summary>
        public void StopAllControllers()
        {
            foreach (var controller in _controllers)
            {
                controller.StopDataAcquisition();
            }
        }

        public void Dispose()
        {
            foreach (var controller in _controllers)
            {
                controller.Dispose();
            }
            _controllers.Clear();
            if (_isDllInitialized)
            {
                _isDllInitialized = false;
            }
            BufferPool.Clear();
        }
    }
}
