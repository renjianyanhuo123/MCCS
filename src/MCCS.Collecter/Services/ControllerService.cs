using MCCS.Collecter.DllNative;
using MCCS.Infrastructure.Helper;

namespace MCCS.Collecter.Services
{
    public class ControllerService : IControllerService
    {
        private static volatile bool _isDllInitialized = false;

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
    }
}
