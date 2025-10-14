using MCCS.Collecter.DllNative;
using MCCS.Infrastructure.Helper;

namespace MCCS.Collecter.Services
{
    public class ControllerService : IControllerService
    {
        private static volatile bool _isDllInitialized = false;

        public ControllerService() { }

        public bool InitializeDll()
        {
            if (_isDllInitialized) return true;
            if (!FileHelper.FileExists(AddressContanst.DllName)) return false;
            var result = POPNetCtrl.NetCtrl01_Init();
            if (result == AddressContanst.OP_SUCCESSFUL)
            { 
                return true;
            }
            else
            {
                throw new Exception($"DLL初始化失败,错误码:{result}");
            }
            _isDllInitialized = true;
        }
    }
}
