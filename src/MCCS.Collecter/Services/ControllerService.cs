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

        public ControllerService()
        {

        }

        public bool InitializeDll()
        {
            if (_isDllInitialized) return true;
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
            if (!_isDllInitialized) throw new ArgumentNullException("DLL未初始化!"); 
            var controller = new BwControllerHardwareDevice(configuration);
            _controllers.Add(controller);
            return true;
        }

        public bool RemoveController(int deviceId)
        {
            if (!_isDllInitialized) throw new ArgumentNullException("DLL未初始化!");
            var controller = _controllers.FirstOrDefault(c => c.DeviceId == deviceId);
            if (controller == null) return true;
            controller.Dispose();
            _controllers.Remove(controller);
            return true;
        }
    }
}
