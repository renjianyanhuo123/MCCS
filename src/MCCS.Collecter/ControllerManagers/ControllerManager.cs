using MCCS.Collecter.ControllerManagers.Entities;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.HardwareDevices; 
using MCCS.Infrastructure.Helper;
using MCCS.Infrastructure.TestModels;

namespace MCCS.Collecter.ControllerManagers
{
    /// <summary>
    /// 控制器管理(务必保持单例)
    /// </summary>
    public sealed class ControllerManager : IControllerManager
    {
        private static volatile bool _isDllInitialized = false;
        private readonly List<ControllerHardwareDeviceBase> _controllers = [];
        private bool _isMock = false;

        public ControllerManager()
        {

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

        public SystemControlState GetControlStateBySignalId(long signalId)
        {
            // 目前还在控制器中, 后面要改进的
            var controller = _controllers.FirstOrDefault(c => c.Signals.Any(s => s.SignalId == signalId));
            if (controller == null) throw new ArgumentNullException("controller no find signal");
            return controller.ControlState;
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
        /// 操作阀门(应该属于设备上的操作;甚至可能由其他设备控制)
        /// 暂时先由控制器掌控吧
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
        /// 这里相当于硬件上的总开关
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
        /// 这里相当于硬件上的总开关
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
