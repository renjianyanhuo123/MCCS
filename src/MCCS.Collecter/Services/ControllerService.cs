using MCCS.Collecter.DllNative;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.HardwareDevices.BwController;
using MCCS.Infrastructure.Helper;

namespace MCCS.Collecter.Services
{
    /// <summary>
    /// 控制器服务(务必保持单例)
    /// </summary>
    public sealed class ControllerService : IControllerService
    {
        private static volatile bool _isDllInitialized = false;
        private readonly List<ControllerHardwareDeviceBase> _controllers = [];
        private bool _isMock = false;

        public ControllerService()
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
